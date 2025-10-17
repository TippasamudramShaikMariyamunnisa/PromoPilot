using PromoPilot.Application.Interfaces;
using PromoPilot.Application.Services;
using PromoPilot.Core.Entities;
using PromoPilot.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PromoPilot.Application.DTOs;

namespace PromoPilot.Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _repository;
        private readonly ILogger<CustomerService> _logger;
        private readonly IAuditLoggingService _auditLoggingService;

        public CustomerService(
            ICustomerRepository repository,
            ILogger<CustomerService> logger,
            IAuditLoggingService auditLoggingService)
        {
            _repository = repository;
            _logger = logger;
            _auditLoggingService = auditLoggingService;
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            _logger.LogInformation("Fetching all customers.");
            return await _repository.GetAllAsync();
        }

        public async Task<Customer> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching customer with ID: {Id}", id);
            return await _repository.GetByIdAsync(id);
        }

        public async Task AddAsync(Customer customer)
        {
            _logger.LogInformation("Adding new customer.");
            await _repository.AddAsync(customer);

            await _auditLoggingService.LogAsync("Create", "Customer", customer.CustomerId.ToString(), customer);
        }

        public async Task UpdateAsync(Customer customer)
        {
            _logger.LogInformation("Updating customer with ID: {Id}", customer.CustomerId);
            await _repository.UpdateAsync(customer);

            await _auditLoggingService.LogAsync("Update", "Customer", customer.CustomerId.ToString(), customer);
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting customer with ID: {Id}", id);
            await _repository.DeleteAsync(id);

            await _auditLoggingService.LogAsync("Delete", "Customer", id.ToString(), new { Deleted = true });
        }
        public async Task<PagedResultDto<Customer>> GetPagedAsync(int pageNumber, int pageSize, string? sortBy, bool sortDesc)
        {
            if (pageNumber <= 0)
                throw new ArgumentException("Page number must be greater than zero.");

            if (pageSize <= 0 || pageSize > 100)
                throw new ArgumentException("Page size must be between 1 and 100.");

            var query = _repository.Query(); // IQueryable<Customer>

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var customerProperties = typeof(Customer).GetProperties().Select(p => p.Name).ToList();

                if (customerProperties.Contains(sortBy))
                {
                    query = sortDesc
                        ? query.OrderByDescending(e => EF.Property<object>(e, sortBy))
                        : query.OrderBy(e => EF.Property<object>(e, sortBy));
                }
                else
                {
                    throw new ArgumentException($"Invalid sort field: {sortBy}. Allowed fields are: {string.Join(", ", customerProperties)}");
                }
            }
            else
            {
                query = query.OrderBy(e => e.CustomerId); // Default sort
            }

            var totalCount = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResultDto<Customer>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}