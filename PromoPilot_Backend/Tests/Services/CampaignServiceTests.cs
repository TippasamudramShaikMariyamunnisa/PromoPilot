using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using PromoPilot.Application.DTOs;
using PromoPilot.Application.Interfaces;
using PromoPilot.Application.Services;
using PromoPilot.Core.Entities;
using PromoPilot.Core.Interfaces;
using PromoPilot.Infrastructure.Data;
using PromoPilot.Infrastructure.Repositories;
using Xunit;

namespace PromoPilot.Tests.Services
{
    public class CampaignServiceTests
    {
        private readonly Mock<ICampaignRepository> _mockRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IAuditLoggingService> _mockAuditLogger;
        private readonly CampaignService _service;

        public CampaignServiceTests()
        {
            _mockRepo = new Mock<ICampaignRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockAuditLogger = new Mock<IAuditLoggingService>();
            _service = new CampaignService(_mockRepo.Object, _mockMapper.Object, _mockAuditLogger.Object);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsMappedDtos()
        {
            // Arrange
            var campaigns = new List<Campaign> { new Campaign(), new Campaign() };
            var campaignDtos = new List<CampaignDto> { new CampaignDto(), new CampaignDto() };

            _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(campaigns);
            _mockMapper.Setup(m => m.Map<IEnumerable<CampaignDto>>(campaigns)).Returns(campaignDtos);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(campaignDtos.Count, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsMappedDto_WhenCampaignExists()
        {
            // Arrange
            var campaign = new Campaign { CampaignId = 1 };
            var campaignDto = new CampaignDto { CampaignId = 1 };

            _mockRepo.Setup(r => r.GetByIdAsync(campaign.CampaignId)).ReturnsAsync(campaign);
            _mockMapper.Setup(m => m.Map<CampaignDto>(campaign)).Returns(campaignDto);

            // Act
            var result = await _service.GetByIdAsync(campaign.CampaignId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(campaignDto.CampaignId, result.CampaignId);
        }

        [Fact]
        public async Task AddAsync_ThrowsException_WhenCampaignOverlaps()
        {
            // Arrange
            var dto = new CampaignDto
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(5),
                StoreList = "Store1,Store2",
                TargetProducts = "Product1,Product2"
            };

            _mockRepo.Setup(r => r.HasOverlappingCampaignAsync(
                dto.StartDate,
                dto.EndDate,
                It.IsAny<List<string>>(),
                It.IsAny<List<string>>()
            )).ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddAsync(dto));
        }

        [Fact]
        public async Task AddAsync_ThrowsException_WhenDuplicateCampaignExists()
        {
            // Arrange
            var dto = new CampaignDto
            {
                Name = "Promo",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(3),
                StoreList = "StoreA",
                TargetProducts = "ProductX"
            };

            _mockRepo.Setup(r => r.HasOverlappingCampaignAsync(
                dto.StartDate,
                dto.EndDate,
                It.IsAny<List<string>>(),
                It.IsAny<List<string>>()
            )).ReturnsAsync(false);

            _mockRepo.Setup(r => r.GetByNameAndDatesAsync(dto.Name, dto.StartDate, dto.EndDate))
                     .ReturnsAsync(new Campaign());

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddAsync(dto));
        }

        [Fact]
        public async Task AddAsync_ReturnsDto_WhenCampaignIsValid()
        {
            // Arrange
            var dto = new CampaignDto
            {
                Name = "New Campaign",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(2),
                StoreList = "Store1",
                TargetProducts = "Product1"
            };

            var campaign = new Campaign { CampaignId = 1, Name = dto.Name };

            _mockRepo.Setup(r => r.HasOverlappingCampaignAsync(
                dto.StartDate,
                dto.EndDate,
                It.IsAny<List<string>>(),
                It.IsAny<List<string>>()
            )).ReturnsAsync(false);

            _mockRepo.Setup(r => r.GetByNameAndDatesAsync(dto.Name, dto.StartDate, dto.EndDate))
                     .ReturnsAsync((Campaign)null);

            _mockMapper.Setup(m => m.Map<Campaign>(dto)).Returns(campaign);
            _mockRepo.Setup(r => r.AddAsync(campaign)).Returns(Task.CompletedTask);
            _mockAuditLogger.Setup(a => a.LogAsync("Create", "Campaign", campaign.CampaignId.ToString(), dto))
                            .Returns(Task.CompletedTask);
            _mockMapper.Setup(m => m.Map<CampaignDto>(campaign)).Returns(dto);

            // Act
            var result = await _service.AddAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dto.Name, result.Name);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsUpdatedDto_WhenCampaignExists()
        {
            // Arrange
            var dto = new CampaignDto { CampaignId = 1, Name = "Updated" };
            var existing = new Campaign { CampaignId = 1, Name = "Old" };

            _mockRepo.Setup(r => r.GetByIdAsync(dto.CampaignId)).ReturnsAsync(existing);
            _mockMapper.Setup(m => m.Map(dto, existing)).Verifiable();
            _mockRepo.Setup(r => r.UpdateAsync(existing)).Returns(Task.CompletedTask);
            _mockAuditLogger.Setup(a => a.LogAsync("Update", "Campaign", dto.CampaignId.ToString(), dto)).Returns(Task.CompletedTask);
            _mockMapper.Setup(m => m.Map<CampaignDto>(existing)).Returns(dto);

            // Act
            var result = await _service.UpdateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dto.Name, result.Name);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsNull_WhenCampaignDoesNotExist()
        {
            // Arrange
            var dto = new CampaignDto { CampaignId = 99 };

            _mockRepo.Setup(r => r.GetByIdAsync(dto.CampaignId)).ReturnsAsync((Campaign)null);

            // Act
            var result = await _service.UpdateAsync(dto);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteAsync_CallsRepositoryAndLogs()
        {
            // Arrange
            var campaignId = 10;

            _mockRepo.Setup(r => r.DeleteAsync(campaignId)).Returns(Task.CompletedTask);
            _mockAuditLogger.Setup(a => a.LogAsync("Delete", "Campaign", campaignId.ToString(), It.IsAny<object>()))
                            .Returns(Task.CompletedTask);

            // Act
            await _service.DeleteAsync(campaignId);

            // Assert
            _mockRepo.Verify(r => r.DeleteAsync(campaignId), Times.Once);
            _mockAuditLogger.Verify(a => a.LogAsync("Delete", "Campaign", campaignId.ToString(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task GetPagedAsync_ReturnsPagedResult_WhenValidInput()
        {
            var options = new DbContextOptionsBuilder<PromoPilotDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new PromoPilotDbContext(options);

            context.Campaigns.AddRange(
                new Campaign
                {
                    CampaignId = 1,
                    Name = "Campaign 1",
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddDays(5),
                    StoreList = "Store1,Store2",
                    TargetProducts = "Product1,Product2"
                },
                new Campaign
                {
                    CampaignId = 2,
                    Name = "Campaign 2",
                    StartDate = DateTime.Today.AddDays(1),
                    EndDate = DateTime.Today.AddDays(6),
                    StoreList = "Store3,Store4",
                    TargetProducts = "Product3,Product4"
                }
            );
            await context.SaveChangesAsync();

            var repo = new CampaignRepository(context);
            var service = new CampaignService(repo, _mockMapper.Object, _mockAuditLogger.Object);

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
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetPagedAsync(pageNumber, pageSize, null, false));
        }

        [Fact]
        public async Task GetPagedAsync_ThrowsException_WhenInvalidSortField()
        {
            // Arrange
            var campaigns = new List<Campaign>
            {
                new Campaign { CampaignId = 1 }
            }.AsQueryable();

            _mockRepo.Setup(r => r.Query()).Returns(campaigns);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.GetPagedAsync(1, 10, "InvalidField", false));
            Assert.Contains("Invalid sort field", ex.Message);
        }
    }
}