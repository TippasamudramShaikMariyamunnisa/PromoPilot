using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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
using Tests.Helpers;
using Xunit;

namespace PromoPilot.Tests.Services
{
    public class BudgetServiceTests
    {
        private readonly Mock<IBudgetRepository> _mockRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IAuditLoggingService> _mockAuditLogger;
        private readonly Mock<ILogger<BudgetService>> _mockLogger;
        private readonly BudgetService _service;

        public BudgetServiceTests()
        {
            _mockRepo = new Mock<IBudgetRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockAuditLogger = new Mock<IAuditLoggingService>();
            _mockLogger = new Mock<ILogger<BudgetService>>();
            _service = new BudgetService(_mockRepo.Object, _mockLogger.Object, _mockMapper.Object, _mockAuditLogger.Object);
        }

        [Fact]
        public async Task GetAllBudgetsAsync_ReturnsBudgets()
        {
            var budgets = new List<Budget> { new Budget(), new Budget() };
            _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(budgets);

            var result = await _service.GetAllBudgetsAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetBudgetDetailsAsync_ReturnsBudget_WhenExists()
        {
            var budget = new Budget { BudgetId = 1 };
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(budget);

            var result = await _service.GetBudgetDetailsAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.BudgetId);
        }

        [Fact]
        public async Task AllocateBudgetAsync_ThrowsException_WhenSpentExceedsAllocated()
        {
            var budget = new Budget { AllocatedAmount = 100, SpentAmount = 150 };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AllocateBudgetAsync(budget));
        }

        [Fact]
        public async Task AllocateBudgetAsync_AddsBudget_WhenValid()
        {
            var budget = new Budget { BudgetId = 1, AllocatedAmount = 200, SpentAmount = 150 };

            _mockRepo.Setup(r => r.AddAsync(budget)).Returns(Task.CompletedTask);
            _mockAuditLogger.Setup(a => a.LogAsync("Create", "Budget", "1", budget)).Returns(Task.CompletedTask);

            await _service.AllocateBudgetAsync(budget);

            _mockRepo.Verify(r => r.AddAsync(budget), Times.Once);
            _mockAuditLogger.Verify(a => a.LogAsync("Create", "Budget", "1", budget), Times.Once);
        }

        [Fact]
        public async Task TrackBudgetSpendAsync_UpdatesBudget()
        {
            var budget = new Budget { BudgetId = 1 };

            _mockRepo.Setup(r => r.UpdateAsync(budget)).Returns(Task.CompletedTask);
            _mockAuditLogger.Setup(a => a.LogAsync("Update", "Budget", "1", budget)).Returns(Task.CompletedTask);

            await _service.TrackBudgetSpendAsync(budget);

            _mockRepo.Verify(r => r.UpdateAsync(budget), Times.Once);
            _mockAuditLogger.Verify(a => a.LogAsync("Update", "Budget", "1", budget), Times.Once);
        }

        [Fact]
        public async Task RemoveBudgetAllocationAsync_DeletesBudget()
        {
            var budgetId = 10;

            _mockRepo.Setup(r => r.DeleteAsync(budgetId)).Returns(Task.CompletedTask);
            _mockAuditLogger.Setup(a => a.LogAsync("Delete", "Budget", budgetId.ToString(), It.IsAny<object>()))
                            .Returns(Task.CompletedTask);

            await _service.RemoveBudgetAllocationAsync(budgetId);

            _mockRepo.Verify(r => r.DeleteAsync(budgetId), Times.Once);
            _mockAuditLogger.Verify(a => a.LogAsync("Delete", "Budget", budgetId.ToString(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ThrowsException_WhenSpentExceedsAllocated()
        {
            var dto = new BudgetDto { BudgetID = 1, AllocatedAmount = 100, SpentAmount = 150 };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(dto));
        }

        [Fact]
        public async Task UpdateAsync_ReturnsUpdatedDto_WhenBudgetExists()
        {
            var dto = new BudgetDto { BudgetID = 1, AllocatedAmount = 200, SpentAmount = 150 };
            var existing = new Budget { BudgetId = 1 };

            _mockRepo.Setup(r => r.GetByIdAsync(dto.BudgetID)).ReturnsAsync(existing);
            _mockMapper.Setup(m => m.Map(dto, existing)).Verifiable();
            _mockRepo.Setup(r => r.UpdateAsync(existing)).Returns(Task.CompletedTask);
            _mockAuditLogger.Setup(a => a.LogAsync("Update", "Budget", "1", dto)).Returns(Task.CompletedTask);
            _mockMapper.Setup(m => m.Map<BudgetDto>(existing)).Returns(dto);

            var result = await _service.UpdateAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(dto.BudgetID, result.BudgetID);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsNull_WhenBudgetDoesNotExist()
        {
            var dto = new BudgetDto { BudgetID = 99 };

            _mockRepo.Setup(r => r.GetByIdAsync(dto.BudgetID)).ReturnsAsync((Budget)null);

            var result = await _service.UpdateAsync(dto);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetPagedAsync_ReturnsPagedResult_WhenValidInput()
        {
            var options = new DbContextOptionsBuilder<PromoPilotDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new PromoPilotDbContext(options);
            context.Budgets.AddRange(new List<Budget>
            {
                new Budget
                {
                    BudgetId = 1,
                    CampaignId = 1,
                    StoreId = 101,
                    AllocatedAmount = 1000,
                    SpentAmount = 500
                },
                new Budget
                {
                    BudgetId = 2,
                    CampaignId = 2,
                    StoreId = 102,
                    AllocatedAmount = 2000,
                    SpentAmount = 1500
                }
            });
            await context.SaveChangesAsync();

            var repo = new BudgetRepository(context);
            var service = new BudgetService(repo, _mockLogger.Object, _mockMapper.Object, _mockAuditLogger.Object);

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
            var budgets = new List<Budget>
            {
                new Budget { BudgetId = 1 }
            }.AsQueryable();

            var asyncBudgets = new TestAsyncEnumerable<Budget>(budgets);
            _mockRepo.Setup(r => r.Query()).Returns(asyncBudgets);

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.GetPagedAsync(1, 10, "InvalidField", false));
            Assert.Contains("Invalid sort field", ex.Message);
        }
    }
}