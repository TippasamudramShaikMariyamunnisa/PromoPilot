using AutoMapper;
using Castle.Core.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using PromoPilot.Application.DTOs;
using PromoPilot.Application.Interfaces;
using PromoPilot.Application.Services;
using PromoPilot.Core.Entities;
using PromoPilot.Core.Interfaces;
using PromoPilot.Infrastructure.Data;
using PromoPilot.Infrastructure.Repositories;
using PromoPilot.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PromoPilot.Tests.Services
{
    public class CampaignReportServiceTests
    {
        private readonly Mock<ICampaignReportRepository> _mockRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IAuditLoggingService> _mockAuditLogger;
        private readonly CampaignReportService _service;
        private readonly Mock<ILogger<CampaignReportService>> _mockLogger;

        public CampaignReportServiceTests()
        {
            _mockRepo = new Mock<ICampaignReportRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockAuditLogger = new Mock<IAuditLoggingService>();
            _service = new CampaignReportService(_mockRepo.Object, _mockMapper.Object, _mockAuditLogger.Object, _mockLogger.Object);
            _mockLogger = new Mock<ILogger<CampaignReportService>>();
        }

        [Fact]
        public async Task GetAllReportsAsync_ReturnsMappedDtos()
        {
            var reports = new List<CampaignReport> { new CampaignReport(), new CampaignReport() };
            var reportDtos = new List<CampaignReportDto> { new CampaignReportDto(), new CampaignReportDto() };

            _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(reports);
            _mockMapper.Setup(m => m.Map<IEnumerable<CampaignReportDto>>(reports)).Returns(reportDtos);

            var result = await _service.GetAllReportsAsync();

            Assert.NotNull(result);
            Assert.Equal(reportDtos.Count, result.Count());
        }

        [Fact]
        public async Task GetReportByIdAsync_ReturnsMappedDto_WhenExists()
        {
            var report = new CampaignReport { ReportId = 1 };
            var reportDto = new CampaignReportDto { ReportID = 1 };

            _mockRepo.Setup(r => r.GetByIdAsync(report.ReportId)).ReturnsAsync(report);
            _mockMapper.Setup(m => m.Map<CampaignReportDto>(report)).Returns(reportDto);

            var result = await _service.GetReportByIdAsync(report.ReportId);

            Assert.NotNull(result);
            Assert.Equal(reportDto.ReportID, result.ReportID);
        }

        [Fact]
        public async Task GetReportByIdAsync_ReturnsNull_WhenNotFound()
        {
            _mockRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((CampaignReport)null);

            var result = await _service.GetReportByIdAsync(99);

            Assert.Null(result);
        }

        [Fact]
        public async Task GenerateReportAsync_ReturnsDto_WhenValid()
        {
            var campaign = new Campaign
            {
                CampaignId = 1,
                Sales = new List<Sale> { new Sale { TotalAmount = 1000 }, new Sale { TotalAmount = 500 } },
                Budgets = new List<Budget> { new Budget { AllocatedAmount = 800 } },
                Engagements = new List<Engagement> { new Engagement(), new Engagement(), new Engagement() }
            };

            var report = new CampaignReport { ReportId = 1, CampaignId = 1 };
            var reportDto = new CampaignReportDto { ReportID = 1 };

            _mockRepo.Setup(r => r.GetCampaignByIdAsync(campaign.CampaignId)).ReturnsAsync(campaign);
            _mockRepo.Setup(r => r.AddAsync(It.IsAny<CampaignReport>())).Returns(Task.CompletedTask);
            _mockAuditLogger.Setup(a => a.LogAsync("Create", "CampaignReport", "1", It.IsAny<object>())).Returns(Task.CompletedTask);
            _mockMapper.Setup(m => m.Map<CampaignReportDto>(It.IsAny<CampaignReport>())).Returns(reportDto);

            var result = await _service.GenerateReportAsync(campaign.CampaignId);

            Assert.NotNull(result);
            Assert.Equal(reportDto.ReportID, result.ReportID);
        }

        [Fact]
        public async Task GenerateReportAsync_ThrowsException_WhenCampaignNotFound()
        {
            _mockRepo.Setup(r => r.GetCampaignByIdAsync(99)).ReturnsAsync((Campaign)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GenerateReportAsync(99));
        }

        [Fact]
        public async Task GenerateReportAsync_ThrowsException_WhenROIIsNegative()
        {
            var campaign = new Campaign
            {
                CampaignId = 1,
                Sales = new List<Sale> { new Sale { TotalAmount = 500 } },
                Budgets = new List<Budget> { new Budget { AllocatedAmount = 1000 } },
                Engagements = new List<Engagement> { new Engagement() }
            };

            _mockRepo.Setup(r => r.GetCampaignByIdAsync(campaign.CampaignId)).ReturnsAsync(campaign);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GenerateReportAsync(campaign.CampaignId));
        }

        [Fact]
        public async Task GenerateReportAsync_ThrowsException_WhenConversionRateExceeds100()
        {
            var campaign = new Campaign
            {
                CampaignId = 1,
                Sales = new List<Sale> { new Sale(), new Sale(), new Sale() },
                Budgets = new List<Budget> { new Budget { AllocatedAmount = 1000 } },
                Engagements = new List<Engagement> { new Engagement() }
            };

            _mockRepo.Setup(r => r.GetCampaignByIdAsync(campaign.CampaignId)).ReturnsAsync(campaign);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GenerateReportAsync(campaign.CampaignId));
        }

        [Fact]
        public async Task GenerateReportAsync_ThrowsException_WhenConversionsExceedReach()
        {
            var campaign = new Campaign
            {
                CampaignId = 1,
                Sales = new List<Sale> { new Sale(), new Sale() },
                Budgets = new List<Budget> { new Budget { AllocatedAmount = 1000 } },
                Engagements = new List<Engagement> { new Engagement() }
            };

            _mockRepo.Setup(r => r.GetCampaignByIdAsync(campaign.CampaignId)).ReturnsAsync(campaign);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GenerateReportAsync(campaign.CampaignId));
        }

        [Fact]
        public async Task CompareByRegionAsync_ReturnsNormalizedRegionPerformance()
        {
            var reports = new List<CampaignReport>
            {
                new CampaignReport
                {
                    CampaignId = 1,
                    Roi = 100,
                    Reach = 200,
                    ConversionRate = 50,
                    Campaign = new Campaign { StoreList = "North,South" }
                }
            };

            _mockRepo.Setup(r => r.GetAllWithCampaignAsync()).ReturnsAsync(reports);

            var result = await _service.CompareByRegionAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, r => Assert.Equal(1, r.CampaignID));
        }

        [Fact]
        public async Task GetAllReportsWithRegionAsync_ReturnsMappedDtos()
        {
            var reports = new List<CampaignReport>
            {
                new CampaignReport
                {
                    CampaignId = 1,
                    Roi = 100,
                    Reach = 200,
                    ConversionRate = 50,
                    Campaign = new Campaign { StoreList = "North" }
                }
            };

            _mockRepo.Setup(r => r.GetAllWithCampaignAsync()).ReturnsAsync(reports);

            var result = await _service.GetAllReportsWithRegionAsync();

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("North", result.First().Region);
        }
        [Fact]
        public async Task GetPagedAsync_ReturnsPagedResult_WhenValidInput()
        {
            var options = new DbContextOptionsBuilder<PromoPilotDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new PromoPilotDbContext(options);

            context.CampaignReports.AddRange(new List<CampaignReport>
            {
                new CampaignReport
                {
                    ReportId = 1,
                    CampaignId = 1,
                    Roi = 100,
                    Reach = 200,
                    ConversionRate = 50,
                    GeneratedDate = DateTime.UtcNow,
                    Campaign = new Campaign
                    {
                        CampaignId = 1,
                        Name = "Campaign 1",
                        StoreList = "North,South",
                        TargetProducts = "Product1,Product2",
                        StartDate = DateTime.Today,
                        EndDate = DateTime.Today.AddDays(5)
                    }
                },
                new CampaignReport
                {
                    ReportId = 2,
                    CampaignId = 2,
                    Roi = 120,
                    Reach = 250,
                    ConversionRate = 60,
                    GeneratedDate = DateTime.UtcNow,
                    Campaign = new Campaign
                    {
                        CampaignId = 2,
                        Name = "Campaign 2",
                        StoreList = "East,West",
                        TargetProducts = "Product3,Product4",
                        StartDate = DateTime.Today.AddDays(1),
                        EndDate = DateTime.Today.AddDays(6)
                    }
                }
            });

            await context.SaveChangesAsync();

            var repo = new CampaignReportRepository(context);
            var service = new CampaignReportService(repo, _mockMapper.Object, _mockAuditLogger.Object, _mockLogger.Object);

            var result = await service.GetPagedAsync(1, 2, null, false);

            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count());
            Assert.Equal(2, result.TotalCount);
        }

        [Theory]
        [InlineData(0, 10)]
        [InlineData(1, 0)]
        [InlineData(1, 101)]
        public async Task GetPagedAsync_ThrowsException_WhenInvalidPaging(int pageNumber, int pageSize)
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetPagedAsync(pageNumber, pageSize, null, false));
        }

        [Fact]
        public async Task GetPagedAsync_ThrowsException_WhenInvalidSortField()
        {
            var reports = new List<CampaignReport>
            {
                new CampaignReport { ReportId = 1 }
            }.AsQueryable();

            var asyncReports = new TestAsyncEnumerable<CampaignReport>(reports);
            _mockRepo.Setup(r => r.Query()).Returns(asyncReports);

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.GetPagedAsync(1, 10, "InvalidField", false));
            Assert.Contains("Invalid sort field", ex.Message);
        }
    }
}