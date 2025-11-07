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
    public class SaleService : ISaleService
    {
        private readonly ISaleRepository _repository;
        private readonly ILogger<SaleService> _logger;
        private readonly IAuditLoggingService _auditLoggingService;

        public SaleService(
            ISaleRepository repository,
            ILogger<SaleService> logger,
            IAuditLoggingService auditLoggingService)
        {
            _repository = repository;
            _logger = logger;
            _auditLoggingService = auditLoggingService;
        }

        public async Task<IEnumerable<Sale>> GetAllAsync()
        {
            _logger.LogInformation("Fetching all sales.");
            return await _repository.GetAllAsync();
        }

        public async Task<Sale> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching sale with ID: {Id}", id);
            return await _repository.GetByIdAsync(id);
        }

        public async Task AddAsync(Sale sale)
        {
            _logger.LogInformation("Adding new sale.");
            await _repository.AddAsync(sale);

            await _auditLoggingService.LogAsync("Create", "Sale", sale.SaleId.ToString(), sale);
        }

        public async Task UpdateAsync(Sale sale)
        {
            _logger.LogInformation("Updating sale with ID: {Id}", sale.SaleId);
            await _repository.UpdateAsync(sale);

            await _auditLoggingService.LogAsync("Update", "Sale", sale.SaleId.ToString(), sale);
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting sale with ID: {Id}", id);
            await _repository.DeleteAsync(id);

            await _auditLoggingService.LogAsync("Delete", "Sale", id.ToString(), new { Deleted = true });
        }
        public async Task<PagedResultDto<Sale>> GetPagedAsync(int pageNumber, int pageSize, string? sortBy, bool sortDesc)
        {
            if (pageNumber <= 0)
                throw new ArgumentException("Page number must be greater than zero.");

            if (pageSize <= 0 || pageSize > 100)
                throw new ArgumentException("Page size must be between 1 and 100.");

            var query = _repository.Query(); // IQueryable<Sales>

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var salesProperties = typeof(Sale).GetProperties().Select(p => p.Name).ToList();

                if (salesProperties.Contains(sortBy))
                {
                    query = sortDesc
                        ? query.OrderByDescending(e => EF.Property<object>(e, sortBy))
                        : query.OrderBy(e => EF.Property<object>(e, sortBy));
                }
                else
                {
                    throw new ArgumentException($"Invalid sort field: {sortBy}. Allowed fields are: {string.Join(", ", salesProperties)}");
                }
            }
            else
            {
                query = query.OrderBy(e => e.SaleId); // Default sort
            }

            var totalCount = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResultDto<Sale>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}