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
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _repoMock;
        private readonly Mock<ILogger<ProductService>> _loggerMock;
        private readonly Mock<IAuditLoggingService> _auditMock;
        private readonly ProductService _service;

        public ProductServiceTests()
        {
            _repoMock = new Mock<IProductRepository>();
            _loggerMock = new Mock<ILogger<ProductService>>();
            _auditMock = new Mock<IAuditLoggingService>();
            _service = new ProductService(_repoMock.Object, _loggerMock.Object, _auditMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllProducts()
        {
            var products = new List<Product> { new Product(), new Product() };
            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(products);

            var result = await _service.GetAllAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsProduct_WhenExists()
        {
            var product = new Product { ProductId = 1 };
            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

            var result = await _service.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.ProductId);
        }

        [Fact]
        public async Task AddAsync_AddsProduct_AndLogsAudit()
        {
            var product = new Product { ProductId = 1, Name = "Test", Category = "TestCat", Price = 99 };

            _repoMock.Setup(r => r.AddAsync(product)).Returns(Task.CompletedTask);
            _auditMock.Setup(a => a.LogAsync("Create", "Product", "1", product)).Returns(Task.CompletedTask);

            await _service.AddAsync(product);

            _repoMock.Verify(r => r.AddAsync(product), Times.Once);
            _auditMock.Verify(a => a.LogAsync("Create", "Product", "1", product), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesProduct_AndLogsAudit()
        {
            var product = new Product { ProductId = 2 };

            _repoMock.Setup(r => r.UpdateAsync(product)).Returns(Task.CompletedTask);
            _auditMock.Setup(a => a.LogAsync("Update", "Product", "2", product)).Returns(Task.CompletedTask);

            await _service.UpdateAsync(product);

            _repoMock.Verify(r => r.UpdateAsync(product), Times.Once);
            _auditMock.Verify(a => a.LogAsync("Update", "Product", "2", product), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_DeletesProduct_AndLogsAudit()
        {
            var id = 3;

            _repoMock.Setup(r => r.DeleteAsync(id)).Returns(Task.CompletedTask);
            _auditMock.Setup(a => a.LogAsync("Delete", "Product", id.ToString(), It.IsAny<object>())).Returns(Task.CompletedTask);

            await _service.DeleteAsync(id);

            _repoMock.Verify(r => r.DeleteAsync(id), Times.Once);
            _auditMock.Verify(a => a.LogAsync("Delete", "Product", id.ToString(), It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task GetPagedAsync_ReturnsPagedResult()
        {
            var options = new DbContextOptionsBuilder<PromoPilotDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new PromoPilotDbContext(options);
            context.Products.AddRange(new List<Product>
            {
                new Product { ProductId = 1, Name = "Product A", Category = "Cat A", Price = 100 },
                new Product { ProductId = 2, Name = "Product B", Category = "Cat B", Price = 200 },
                new Product { ProductId = 3, Name = "Product C", Category = "Cat C", Price = 300 }
            });
            await context.SaveChangesAsync();

            var repo = new ProductRepository(context);
            var service = new ProductService(repo, _loggerMock.Object, _auditMock.Object);

            var result = await service.GetPagedAsync(1, 2, "ProductId", false);

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
            context.Products.Add(new Product { ProductId = 1, Name = "Product A", Category = "Cat A", Price = 100 });
            await context.SaveChangesAsync();

            var repo = new ProductRepository(context);
            var service = new ProductService(repo, _loggerMock.Object, _auditMock.Object);

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.GetPagedAsync(1, 10, "InvalidField", false));
            Assert.Contains("Invalid sort field", ex.Message);
        }
    }
}