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
    public class SaleServiceTests
    {
        private readonly Mock<ISaleRepository> _repoMock;
        private readonly Mock<ILogger<SaleService>> _loggerMock;
        private readonly Mock<IAuditLoggingService> _auditMock;
        private readonly SaleService _service;

        public SaleServiceTests()
        {
            _repoMock = new Mock<ISaleRepository>();
            _loggerMock = new Mock<ILogger<SaleService>>();
            _auditMock = new Mock<IAuditLoggingService>();
            _service = new SaleService(_repoMock.Object, _loggerMock.Object, _auditMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllSales()
        {
            var sales = new List<Sale> { new Sale(), new Sale() };
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(sales);

            var result = await _service.GetAllAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsSale_WhenExists()
        {
            var sale = new Sale { SaleId = 1 };
            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(sale);

            var result = await _service.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.SaleId);
        }

        [Fact]
        public async Task AddAsync_AddsSale_AndLogsAudit()
        {
            var sale = new Sale { SaleId = 1, Quantity = 2, TotalAmount = 200, TransactionId = "TX123", PaymentMethod = "Card", PaymentStatus = "Paid", PaymentDate = DateTime.UtcNow };

            _repoMock.Setup(r => r.AddAsync(sale)).Returns(Task.CompletedTask);
            _auditMock.Setup(a => a.LogAsync("Create", "Sale", "1", sale)).Returns(Task.CompletedTask);

            await _service.AddAsync(sale);

            _repoMock.Verify(r => r.AddAsync(sale), Times.Once);
            _auditMock.Verify(a => a.LogAsync("Create", "Sale", "1", sale), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesSale_AndLogsAudit()
        {
            var sale = new Sale { SaleId = 2 };

            _repoMock.Setup(r => r.UpdateAsync(sale)).Returns(Task.CompletedTask);
            _auditMock.Setup(a => a.LogAsync("Update", "Sale", "2", sale)).Returns(Task.CompletedTask);

            await _service.UpdateAsync(sale);

            _repoMock.Verify(r => r.UpdateAsync(sale), Times.Once);
            _auditMock.Verify(a => a.LogAsync("Update", "Sale", "2", sale), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_DeletesSale_AndLogsAudit()
        {
            var id = 3;

            _repoMock.Setup(r => r.DeleteAsync(id)).Returns(Task.CompletedTask);
            _auditMock.Setup(a => a.LogAsync("Delete", "Sale", id.ToString(), It.IsAny<object>())).Returns(Task.CompletedTask);

            await _service.DeleteAsync(id);

            _repoMock.Verify(r => r.DeleteAsync(id), Times.Once);
            _auditMock.Verify(a => a.LogAsync("Delete", "Sale", id.ToString(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task GetPagedAsync_ReturnsPagedResult()
        {
            var options = new DbContextOptionsBuilder<PromoPilotDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new PromoPilotDbContext(options);
            context.Sales.AddRange(new List<Sale>
            {
                new Sale { SaleId = 1, CustomerId = 1, ProductId = 1, CampaignId = 1, StoreId = 101, Quantity = 2, TotalAmount = 200, SaleDate = DateTime.Today, TransactionId = "TX1", PaymentMethod = "Card", PaymentStatus = "Paid", PaymentDate = DateTime.Today },
                new Sale { SaleId = 2, CustomerId = 2, ProductId = 2, CampaignId = 1, StoreId = 102, Quantity = 1, TotalAmount = 100, SaleDate = DateTime.Today, TransactionId = "TX2", PaymentMethod = "UPI", PaymentStatus = "Paid", PaymentDate = DateTime.Today },
                new Sale { SaleId = 3, CustomerId = 3, ProductId = 3, CampaignId = 2, StoreId = 103, Quantity = 3, TotalAmount = 300, SaleDate = DateTime.Today, TransactionId = "TX3", PaymentMethod = "Cash", PaymentStatus = "Pending", PaymentDate = DateTime.Today }
            });
            await context.SaveChangesAsync();

            var repo = new SaleRepository(context);
            var service = new SaleService(repo, _loggerMock.Object, _auditMock.Object);

            var result = await service.GetPagedAsync(1, 2, "SaleId", false);

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
            context.Sales.Add(new Sale { SaleId = 1, CustomerId = 1, ProductId = 1, CampaignId = 1, StoreId = 101, Quantity = 2, TotalAmount = 200, SaleDate = DateTime.Today, TransactionId = "TX1", PaymentMethod = "Card", PaymentStatus = "Paid", PaymentDate = DateTime.Today });
            await context.SaveChangesAsync();

            var repo = new SaleRepository(context);
            var service = new SaleService(repo, _loggerMock.Object, _auditMock.Object);

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.GetPagedAsync(1, 10, "InvalidField", false));
            Assert.Contains("Invalid sort field", ex.Message);
        }
    }
}