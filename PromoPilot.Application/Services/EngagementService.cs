using System;
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
    public class EngagementService : IEngagementService
    {
        private readonly IEngagementRepository _repository;
        private readonly ILogger<EngagementService> _logger;
        private readonly IAuditLoggingService _auditLoggingService;

        public EngagementService(
            IEngagementRepository repository,
            ILogger<EngagementService> logger,
            IAuditLoggingService auditLoggingService)
        {
            _repository = repository;
            _logger = logger;
            _auditLoggingService = auditLoggingService;
        }

        public async Task<IEnumerable<Engagement>> GetAllAsync()
        {
            _logger.LogInformation("Fetching all engagements.");
            return await _repository.GetAllAsync();
        }

        public async Task<Engagement> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching engagement with ID: {Id}", id);
            return await _repository.GetByIdAsync(id);
        }

        public async Task AddAsync(Engagement engagement)
        {
            if (engagement.RedemptionCount < 0)
                throw new InvalidOperationException("Redemption count cannot be negative.");

            if (engagement.PurchaseValue < 0.01m)
                throw new InvalidOperationException("Purchase value must be at least 0.01.");

            var campaignExists = await _repository.CampaignExistsAsync(engagement.CampaignId);
            if (!campaignExists)
                throw new InvalidOperationException($"CampaignID {engagement.CampaignId} does not exist.");

            var customerExists = await _repository.CustomerExistsAsync(engagement.CustomerId);
            if (!customerExists)
                throw new InvalidOperationException($"CustomerID {engagement.CustomerId} does not exist.");

            _logger.LogInformation("Adding new engagement.");
            await _repository.AddAsync(engagement);

            await _auditLoggingService.LogAsync("Create", "Engagement", engagement.EngagementId.ToString(), engagement);
        }

        public async Task UpdateAsync(Engagement engagement)
        {
            _logger.LogInformation("Updating engagement with ID: {Id}", engagement.EngagementId);
            await _repository.UpdateAsync(engagement);

            await _auditLoggingService.LogAsync("Update", "Engagement", engagement.EngagementId.ToString(), engagement);
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting engagement with ID: {Id}", id);
            await _repository.DeleteAsync(id);

            await _auditLoggingService.LogAsync("Delete", "Engagement", id.ToString(), new { Deleted = true });
        }
        public async Task<PagedResultDto<Engagement>> GetPagedAsync(int pageNumber, int pageSize, string? sortBy, bool sortDesc)
        {
            if (pageNumber <= 0)
                throw new ArgumentException("Page number must be greater than zero.");

            if (pageSize <= 0 || pageSize > 100)
                throw new ArgumentException("Page size must be between 1 and 100.");

            var query = _repository.Query(); // IQueryable<Engagement>

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var engagementProperties = typeof(Engagement).GetProperties().Select(p => p.Name).ToList();

                if (engagementProperties.Contains(sortBy))
                {
                    query = sortDesc
                        ? query.OrderByDescending(e => EF.Property<object>(e, sortBy))
                        : query.OrderBy(e => EF.Property<object>(e, sortBy));
                }
                else
                {
                    throw new ArgumentException($"Invalid sort field: {sortBy}. Allowed fields are: {string.Join(", ", engagementProperties)}");
                }
            }
            else
            {
                query = query.OrderBy(e => e.EngagementId); // Default sort
            }

            var totalCount = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResultDto<Engagement>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}