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
    public class CustomerServiceTests
    {
        private readonly Mock<ICustomerRepository> _repositoryMock;
        private readonly Mock<ILogger<CustomerService>> _loggerMock;
        private readonly Mock<IAuditLoggingService> _auditMock;
        private readonly CustomerService _service;

        public CustomerServiceTests()
        {
            _repositoryMock = new Mock<ICustomerRepository>();
            _loggerMock = new Mock<ILogger<CustomerService>>();
            _auditMock = new Mock<IAuditLoggingService>();
            _service = new CustomerService(_repositoryMock.Object, _loggerMock.Object, _auditMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllCustomers()
        {
            var customers = new List<Customer>
            {
                new Customer { CustomerId = 1, Name = "Test User", Email = "test@example.com" }
            };

            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(customers);

            var result = await _service.GetAllAsync();

            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCustomer_WhenExists()
        {
            var customer = new Customer { CustomerId = 1, Name = "Test User", Email = "test@example.com" };
            _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(customer);

            var result = await _service.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Test User", result.Name);
        }

        [Fact]
        public async Task AddAsync_CallsRepositoryAndAudit()
        {
            var customer = new Customer { CustomerId = 2, Name = "New User", Email = "new@example.com" };

            await _service.AddAsync(customer);

            _repositoryMock.Verify(r => r.AddAsync(customer), Times.Once);
            _auditMock.Verify(a => a.LogAsync("Create", "Customer", "2", customer), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_CallsRepositoryAndAudit()
        {
            var customer = new Customer { CustomerId = 3, Name = "Updated User", Email = "updated@example.com" };

            await _service.UpdateAsync(customer);

            _repositoryMock.Verify(r => r.UpdateAsync(customer), Times.Once);
            _auditMock.Verify(a => a.LogAsync("Update", "Customer", "3", customer), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_CallsRepositoryAndAudit()
        {
            await _service.DeleteAsync(4);

            _repositoryMock.Verify(r => r.DeleteAsync(4), Times.Once);
            _auditMock.Verify(a => a.LogAsync("Delete", "Customer", "4", It.Is<object>(o => o.ToString().Contains("Deleted"))), Times.Once);
        }


        [Fact]
        public async Task GetPagedAsync_ReturnsPagedResult()
        {
            var options = new DbContextOptionsBuilder<PromoPilotDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new PromoPilotDbContext(options);

            context.Customers.AddRange(new List<Customer>
            {
                new Customer { CustomerId = 1, Name = "Alice", Email = "alice@example.com" },
                new Customer { CustomerId = 2, Name = "Bob", Email = "bob@example.com" },
                new Customer { CustomerId = 3, Name = "Charlie", Email = "charlie@example.com" }
            });

            await context.SaveChangesAsync();

            var repo = new CustomerRepository(context);
            var logger = new Mock<ILogger<CustomerService>>();
            var audit = new Mock<IAuditLoggingService>();
            var service = new CustomerService(repo, logger.Object, audit.Object);

            var result = await service.GetPagedAsync(1, 2, "CustomerId", false);

            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count());
            Assert.Equal(3, result.TotalCount);
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(2, result.PageSize);
        }

        [Theory]
        [InlineData(0, 5)]
        [InlineData(1, 0)]
        [InlineData(1, 101)]
        public async Task GetPagedAsync_ThrowsArgumentException_ForInvalidInputs(int pageNumber, int pageSize)
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.GetPagedAsync(pageNumber, pageSize, null, false));
        }

        [Fact]
        public async Task GetPagedAsync_ThrowsArgumentException_ForInvalidSortField()
        {
            var data = new List<Customer>
            {
                new Customer { CustomerId = 1, Name = "User A", Email = "a@example.com" }
            }.AsQueryable();

            _repositoryMock.Setup(r => r.Query()).Returns(data);

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.GetPagedAsync(1, 5, "InvalidField", false));

            Assert.Contains("Invalid sort field", ex.Message);
        }
    }
}