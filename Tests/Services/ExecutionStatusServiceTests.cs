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
    public class ExecutionStatusServiceTests
    {
        private readonly Mock<IExecutionStatusRepository> _repoMock;
        private readonly Mock<ILogger<ExecutionStatusService>> _loggerMock;
        private readonly Mock<IAuditLoggingService> _auditMock;
        private readonly ExecutionStatusService _service;

        public ExecutionStatusServiceTests()
        {
            _repoMock = new Mock<IExecutionStatusRepository>();
            _loggerMock = new Mock<ILogger<ExecutionStatusService>>();
            _auditMock = new Mock<IAuditLoggingService>();
            _service = new ExecutionStatusService(_repoMock.Object, _loggerMock.Object, _auditMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllStatuses()
        {
            var statuses = new List<ExecutionStatus> { new ExecutionStatus(), new ExecutionStatus() };
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(statuses);

            var result = await _service.GetAllAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsStatus_WhenExists()
        {
            var status = new ExecutionStatus { StatusId = 1 };
            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(status);

            var result = await _service.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.StatusId);
        }

        [Fact]
        public async Task AddAsync_ThrowsException_WhenStatusIsInvalid()
        {
            var status = new ExecutionStatus { Status = "Unknown", Feedback = "Valid feedback", CampaignId = 1 };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddAsync(status));
        }

        [Fact]
        public async Task AddAsync_ThrowsException_WhenFeedbackIsTooShort()
        {
            var status = new ExecutionStatus { Status = "Pending", Feedback = "Too short", CampaignId = 1 };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddAsync(status));
        }

        [Fact]
        public async Task AddAsync_ThrowsException_WhenCampaignDoesNotExist()
        {
            var status = new ExecutionStatus { Status = "Pending", Feedback = "Valid feedback", CampaignId = 99 };

            _repoMock.Setup(r => r.CampaignExistsAsync(99)).ReturnsAsync(false);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddAsync(status));
        }

        [Fact]
        public async Task AddAsync_AddsStatus_WhenValid()
        {
            var status = new ExecutionStatus { StatusId = 1, Status = "Completed", Feedback = "Execution done", CampaignId = 1 };

            _repoMock.Setup(r => r.CampaignExistsAsync(1)).ReturnsAsync(true);
            _repoMock.Setup(r => r.AddAsync(status)).Returns(Task.CompletedTask);
            _auditMock.Setup(a => a.LogAsync("Create", "ExecutionStatus", "1", status)).Returns(Task.CompletedTask);

            await _service.AddAsync(status);

            _repoMock.Verify(r => r.AddAsync(status), Times.Once);
            _auditMock.Verify(a => a.LogAsync("Create", "ExecutionStatus", "1", status), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesStatus()
        {
            var status = new ExecutionStatus { StatusId = 2 };

            _repoMock.Setup(r => r.UpdateAsync(status)).Returns(Task.CompletedTask);
            _auditMock.Setup(a => a.LogAsync("Update", "ExecutionStatus", "2", status)).Returns(Task.CompletedTask);

            await _service.UpdateAsync(status);

            _repoMock.Verify(r => r.UpdateAsync(status), Times.Once);
            _auditMock.Verify(a => a.LogAsync("Update", "ExecutionStatus", "2", status), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_DeletesStatus()
        {
            var id = 3;

            _repoMock.Setup(r => r.DeleteAsync(id)).Returns(Task.CompletedTask);
            _auditMock.Setup(a => a.LogAsync("Delete", "ExecutionStatus", id.ToString(), It.IsAny<object>())).Returns(Task.CompletedTask);

            await _service.DeleteAsync(id);

            _repoMock.Verify(r => r.DeleteAsync(id), Times.Once);
            _auditMock.Verify(a => a.LogAsync("Delete", "ExecutionStatus", id.ToString(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task GetPagedAsync_ReturnsPagedResult()
        {
            var options = new DbContextOptionsBuilder<PromoPilotDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new PromoPilotDbContext(options);
            context.ExecutionStatuses.AddRange(new List<ExecutionStatus>
            {
                new ExecutionStatus { StatusId = 1, CampaignId = 1, StoreId = 101, Status = "Pending", Feedback = "Waiting for approval" },
                new ExecutionStatus { StatusId = 2, CampaignId = 1, StoreId = 102, Status = "InProgress", Feedback = "Execution started" },
                new ExecutionStatus { StatusId = 3, CampaignId = 2, StoreId = 103, Status = "Completed", Feedback = "Execution completed successfully" }
            });
            await context.SaveChangesAsync();

            var repo = new ExecutionStatusRepository(context);
            var service = new ExecutionStatusService(repo, _loggerMock.Object, _auditMock.Object);

            var result = await service.GetPagedAsync(1, 2, "StatusId", false);

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
            context.ExecutionStatuses.Add(new ExecutionStatus { StatusId = 1, CampaignId = 1, StoreId = 101, Status = "Pending", Feedback = "Waiting for approval" });
            await context.SaveChangesAsync();

            var repo = new ExecutionStatusRepository(context);
            var service = new ExecutionStatusService(repo, _loggerMock.Object, _auditMock.Object);

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.GetPagedAsync(1, 10, "InvalidField", false));
            Assert.Contains("Invalid sort field", ex.Message);
        }
    }
}