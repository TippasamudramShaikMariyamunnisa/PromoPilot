using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PromoPilot.Application.DTOs;
using PromoPilot.Application.Interfaces;
using PromoPilot.Application.Services;
using PromoPilot.Core.Entities;
using PromoPilot.Core.Interfaces;

namespace PromoPilot.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
        private readonly ILogger<ProductService> _logger;
        private readonly IAuditLoggingService _auditLoggingService;

        public ProductService(
            IProductRepository repository,
            ILogger<ProductService> logger,
            IAuditLoggingService auditLoggingService)
        {
            _repository = repository;
            _logger = logger;
            _auditLoggingService = auditLoggingService;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            _logger.LogInformation("Fetching all products.");
            return await _repository.GetAllAsync();
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching product with ID: {Id}", id);
            return await _repository.GetByIdAsync(id);
        }

        public async Task AddAsync(Product product)
        {
            _logger.LogInformation("Adding new product.");
            await _repository.AddAsync(product);

            await _auditLoggingService.LogAsync("Create", "Product", product.ProductId.ToString(), product);
        }

        public async Task UpdateAsync(Product product)
        {
            _logger.LogInformation("Updating product with ID: {Id}", product.ProductId);
            await _repository.UpdateAsync(product);

            await _auditLoggingService.LogAsync("Update", "Product", product.ProductId.ToString(), product);
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting product with ID: {Id}", id);
            await _repository.DeleteAsync(id);

            await _auditLoggingService.LogAsync("Delete", "Product", id.ToString(), new { Deleted = true });
        }
        public async Task<PagedResultDto<Product>> GetPagedAsync(int pageNumber, int pageSize, string? sortBy, bool sortDesc)
        {
            if (pageNumber <= 0)
                throw new ArgumentException("Page number must be greater than zero.");

            if (pageSize <= 0 || pageSize > 100)
                throw new ArgumentException("Page size must be between 1 and 100.");

            var query = _repository.Query(); // IQueryable<Product>

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var productProperties = typeof(Product).GetProperties().Select(p => p.Name).ToList();

                if (productProperties.Contains(sortBy))
                {
                    query = sortDesc
                        ? query.OrderByDescending(e => EF.Property<object>(e, sortBy))
                        : query.OrderBy(e => EF.Property<object>(e, sortBy));
                }
                else
                {
                    throw new ArgumentException($"Invalid sort field: {sortBy}. Allowed fields are: {string.Join(", ", productProperties)}");
                }
            }
            else
            {
                query = query.OrderBy(e => e.ProductId); // Default sort
            }

            var totalCount = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResultDto<Product>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}