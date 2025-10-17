using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using Xunit;

namespace PromoPilot.Tests.Services
{
    public class EngagementServiceTests
    {
        private readonly Mock<IEngagementRepository> _repoMock;
        private readonly Mock<ILogger<EngagementService>> _loggerMock;
        private readonly Mock<IAuditLoggingService> _auditMock;
        private readonly EngagementService _service;

        public EngagementServiceTests()
        {
            _repoMock = new Mock<IEngagementRepository>();
            _loggerMock = new Mock<ILogger<EngagementService>>();
            _auditMock = new Mock<IAuditLoggingService>();
            _service = new EngagementService(_repoMock.Object, _loggerMock.Object, _auditMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllEngagements()
        {
            var engagements = new List<Engagement> { new Engagement(), new Engagement() };
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(engagements);

            var result = await _service.GetAllAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsEngagement_WhenExists()
        {
            var engagement = new Engagement { EngagementId = 1 };
            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(engagement);

            var result = await _service.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.EngagementId);
        }

        [Fact]
        public async Task AddAsync_ThrowsException_WhenRedemptionCountIsNegative()
        {
            var engagement = new Engagement { RedemptionCount = -1, PurchaseValue = 100, CampaignId = 1, CustomerId = 1 };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddAsync(engagement));
        }

        [Fact]
        public async Task AddAsync_ThrowsException_WhenPurchaseValueIsTooLow()
        {
            var engagement = new Engagement { RedemptionCount = 1, PurchaseValue = 0.001m, CampaignId = 1, CustomerId = 1 };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddAsync(engagement));
        }

        [Fact]
        public async Task AddAsync_ThrowsException_WhenCampaignDoesNotExist()
        {
            var engagement = new Engagement { RedemptionCount = 1, PurchaseValue = 100, CampaignId = 99, CustomerId = 1 };

            _repoMock.Setup(r => r.CampaignExistsAsync(99)).ReturnsAsync(false);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddAsync(engagement));
        }

        [Fact]
        public async Task AddAsync_ThrowsException_WhenCustomerDoesNotExist()
        {
            var engagement = new Engagement { RedemptionCount = 1, PurchaseValue = 100, CampaignId = 1, CustomerId = 99 };

            _repoMock.Setup(r => r.CampaignExistsAsync(1)).ReturnsAsync(true);
            _repoMock.Setup(r => r.CustomerExistsAsync(99)).ReturnsAsync(false);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddAsync(engagement));
        }

        [Fact]
        public async Task AddAsync_AddsEngagement_WhenValid()
        {
            var engagement = new Engagement { EngagementId = 1, RedemptionCount = 1, PurchaseValue = 100, CampaignId = 1, CustomerId = 1 };

            _repoMock.Setup(r => r.CampaignExistsAsync(1)).ReturnsAsync(true);
            _repoMock.Setup(r => r.CustomerExistsAsync(1)).ReturnsAsync(true);
            _repoMock.Setup(r => r.AddAsync(engagement)).Returns(Task.CompletedTask);
            _auditMock.Setup(a => a.LogAsync("Create", "Engagement", "1", engagement)).Returns(Task.CompletedTask);

            await _service.AddAsync(engagement);

            _repoMock.Verify(r => r.AddAsync(engagement), Times.Once);
            _auditMock.Verify(a => a.LogAsync("Create", "Engagement", "1", engagement), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesEngagement()
        {
            var engagement = new Engagement { EngagementId = 2 };

            _repoMock.Setup(r => r.UpdateAsync(engagement)).Returns(Task.CompletedTask);
            _auditMock.Setup(a => a.LogAsync("Update", "Engagement", "2", engagement)).Returns(Task.CompletedTask);

            await _service.UpdateAsync(engagement);

            _repoMock.Verify(r => r.UpdateAsync(engagement), Times.Once);
            _auditMock.Verify(a => a.LogAsync("Update", "Engagement", "2", engagement), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_DeletesEngagement()
        {
            var id = 3;

            _repoMock.Setup(r => r.DeleteAsync(id)).Returns(Task.CompletedTask);
            _auditMock.Setup(a => a.LogAsync("Delete", "Engagement", id.ToString(), It.Is<object>(o => o.ToString().Contains("Deleted")))).Returns(Task.CompletedTask);

            await _service.DeleteAsync(id);

            _repoMock.Verify(r => r.DeleteAsync(id), Times.Once);
            _auditMock.Verify(a => a.LogAsync("Delete", "Engagement", id.ToString(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task GetPagedAsync_ReturnsPagedResult()
        {
            var options = new DbContextOptionsBuilder<PromoPilotDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new PromoPilotDbContext(options);
            context.Engagements.AddRange(new List<Engagement>
            {
                new Engagement { EngagementId = 1, CampaignId = 1, CustomerId = 1, RedemptionCount = 2, PurchaseValue = 100 },
                new Engagement { EngagementId = 2, CampaignId = 1, CustomerId = 2, RedemptionCount = 3, PurchaseValue = 150 },
                new Engagement { EngagementId = 3, CampaignId = 2, CustomerId = 3, RedemptionCount = 1, PurchaseValue = 200 }
            });
            await context.SaveChangesAsync();

            var repo = new EngagementRepository(context);
            var service = new EngagementService(repo, _loggerMock.Object, _auditMock.Object);

            var result = await service.GetPagedAsync(1, 2, "EngagementId", false);

            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count());
            Assert.Equal(3, result.TotalCount);
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
            var options = new DbContextOptionsBuilder<PromoPilotDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new PromoPilotDbContext(options);
            context.Engagements.Add(new Engagement { EngagementId = 1, CampaignId = 1, CustomerId = 1, RedemptionCount = 2, PurchaseValue = 100 });
            await context.SaveChangesAsync();

            var repo = new EngagementRepository(context);
            var service = new EngagementService(repo, _loggerMock.Object, _auditMock.Object);

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.GetPagedAsync(1, 10, "InvalidField", false));
            Assert.Contains("Invalid sort field", ex.Message);
        }
    }
}