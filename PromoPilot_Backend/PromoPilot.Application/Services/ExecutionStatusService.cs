using System;
using System.Collections.Generic;
using System.Linq;
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
    public class ExecutionStatusService : IExecutionStatusService
    {
        private readonly IExecutionStatusRepository _repository;
        private readonly ILogger<ExecutionStatusService> _logger;
        private readonly IAuditLoggingService _auditLoggingService;

        public ExecutionStatusService(
            IExecutionStatusRepository repository,
            ILogger<ExecutionStatusService> logger,
            IAuditLoggingService auditLoggingService)
        {
            _repository = repository;
            _logger = logger;
            _auditLoggingService = auditLoggingService;
        }

        public async Task<IEnumerable<ExecutionStatus>> GetAllAsync()
        {
            _logger.LogInformation("Fetching all execution statuses.");
            return await _repository.GetAllAsync();
        }

        public async Task<ExecutionStatus> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching execution status with ID: {Id}", id);
            return await _repository.GetByIdAsync(id);
        }

        public async Task AddAsync(ExecutionStatus status)
        {
            if (!new[] { "Pending", "InProgress", "Completed" }.Contains(status.Status))
                throw new InvalidOperationException("Invalid status value.");

            if (!string.IsNullOrWhiteSpace(status.Feedback) && status.Feedback.Length < 10)
                throw new InvalidOperationException("Feedback must be at least 10 characters if provided.");

            var campaignExists = await _repository.CampaignExistsAsync(status.CampaignId);
            if (!campaignExists)
                throw new InvalidOperationException($"CampaignID {status.CampaignId} does not exist.");

            _logger.LogInformation("Adding new execution status.");
            await _repository.AddAsync(status);

            await _auditLoggingService.LogAsync("Create", "ExecutionStatus", status.StatusId.ToString(), status);
        }

        public async Task UpdateAsync(ExecutionStatus status)
        {
            _logger.LogInformation("Updating execution status with ID: {Id}", status.StatusId);
            await _repository.UpdateAsync(status);

            await _auditLoggingService.LogAsync("Update", "ExecutionStatus", status.StatusId.ToString(), status);
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting execution status with ID: {Id}", id);
            await _repository.DeleteAsync(id);

            await _auditLoggingService.LogAsync("Delete", "ExecutionStatus", id.ToString(), new { Deleted = true });
        }
        public async Task<PagedResultDto<ExecutionStatus>> GetPagedAsync(int pageNumber, int pageSize, string? sortBy, bool sortDesc)
        {
            if (pageNumber <= 0)
                throw new ArgumentException("Page number must be greater than zero.");

            if (pageSize <= 0 || pageSize > 100)
                throw new ArgumentException("Page size must be between 1 and 100.");

            var query = _repository.Query(); // IQueryable<ExecutionStatus>

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var statusProperties = typeof(ExecutionStatus).GetProperties().Select(p => p.Name).ToList();

                if (statusProperties.Contains(sortBy))
                {
                    query = sortDesc
                        ? query.OrderByDescending(e => EF.Property<object>(e, sortBy))
                        : query.OrderBy(e => EF.Property<object>(e, sortBy));
                }
                else
                {
                    throw new ArgumentException($"Invalid sort field: {sortBy}. Allowed fields are: {string.Join(", ", statusProperties)}");
                }
            }
            else
            {
                query = query.OrderBy(e => e.StatusId); // Default sort
            }

            var totalCount = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResultDto<ExecutionStatus>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}
