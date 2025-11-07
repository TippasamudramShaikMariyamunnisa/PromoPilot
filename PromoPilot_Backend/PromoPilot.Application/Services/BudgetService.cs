using AutoMapper;
using Microsoft.Extensions.Logging;
using PromoPilot.Application.DTOs;
using PromoPilot.Application.Interfaces;
using PromoPilot.Application.Services;
using PromoPilot.Core.Entities;
using PromoPilot.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PromoPilot.Application.Services
{
    public class BudgetService : IBudgetService
    {
        private readonly IBudgetRepository _repository;
        private readonly ILogger<BudgetService> _logger;
        private readonly IMapper _mapper;
        private readonly IAuditLoggingService _auditLoggingService;

        public BudgetService(
            IBudgetRepository repository,
            ILogger<BudgetService> logger,
            IMapper mapper,
            IAuditLoggingService auditLoggingService)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _auditLoggingService = auditLoggingService;
        }

        public async Task<IEnumerable<Budget>> GetAllBudgetsAsync()
        {
            _logger.LogInformation("Retrieving all budget allocations.");
            return await _repository.GetAllAsync();
        }

        public async Task<Budget> GetBudgetDetailsAsync(int id)
        {
            _logger.LogInformation("Retrieving budget allocation for ID: {Id}", id);
            return await _repository.GetByIdAsync(id);
        }

        public async Task AllocateBudgetAsync(Budget budget)
        {
            if (budget.SpentAmount > budget.AllocatedAmount)
            {
                _logger.LogWarning("Validation failed during budget creation: SpentAmount ({Spent}) exceeds AllocatedAmount ({Allocated})", budget.SpentAmount, budget.AllocatedAmount);
                throw new InvalidOperationException("Spent amount cannot exceed allocated budget.");
            }

            _logger.LogInformation("Allocating budget for CampaignID: {CampaignId}, StoreID: {StoreId}", budget.CampaignId, budget.StoreId);
            await _repository.AddAsync(budget);

            await _auditLoggingService.LogAsync("Create", "Budget", budget.BudgetId.ToString(), budget);
        }

        public async Task TrackBudgetSpendAsync(Budget budget)
        {
            _logger.LogInformation("Tracking spend for BudgetID: {BudgetId}", budget.BudgetId);
            await _repository.UpdateAsync(budget);

            await _auditLoggingService.LogAsync("Update", "Budget", budget.BudgetId.ToString(), budget);
        }

        public async Task RemoveBudgetAllocationAsync(int id)
        {
            _logger.LogInformation("Removing budget allocation for BudgetID: {BudgetId}", id);
            await _repository.DeleteAsync(id);

            await _auditLoggingService.LogAsync("Delete", "Budget", id.ToString(), new { Deleted = true });
        }

        public async Task<BudgetDto> UpdateAsync(BudgetDto dto)
        {
            if (dto.SpentAmount > dto.AllocatedAmount)
            {
                _logger.LogWarning("Validation failed during budget update: SpentAmount ({Spent}) exceeds AllocatedAmount ({Allocated})", dto.SpentAmount, dto.AllocatedAmount);
                throw new InvalidOperationException("Spent amount cannot exceed allocated budget.");
            }

            var existing = await _repository.GetByIdAsync(dto.BudgetID);
            if (existing == null) return null;

            _mapper.Map(dto, existing);
            await _repository.UpdateAsync(existing);

            await _auditLoggingService.LogAsync("Update", "Budget", dto.BudgetID.ToString(), dto);

            return _mapper.Map<BudgetDto>(existing);
        }
        public async Task<PagedResultDto<Budget>> GetPagedAsync(int pageNumber, int pageSize, string? sortBy, bool sortDesc)
        {
            // Validate paging parameters
            if (pageNumber <= 0)
                throw new ArgumentException("Page number must be greater than zero.");

            if (pageSize <= 0 || pageSize > 100)
                throw new ArgumentException("Page size must be between 1 and 100.");

            var query = _repository.Query(); // IQueryable<Budget>

            // Validate and apply sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var budgetProperties = typeof(Budget).GetProperties().Select(p => p.Name).ToList();

                if (budgetProperties.Contains(sortBy))
                {
                    query = sortDesc
                        ? query.OrderByDescending(e => EF.Property<object>(e, sortBy))
                        : query.OrderBy(e => EF.Property<object>(e, sortBy));
                }
                else
                {
                    throw new ArgumentException($"Invalid sort field: {sortBy}. Allowed fields are: {string.Join(", ", budgetProperties)}");
                }
            }
            else
            {
                // Default sorting by BudgetID
                query = query.OrderBy(e => e.BudgetId);
            }

            var totalCount = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResultDto<Budget>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

    }
}