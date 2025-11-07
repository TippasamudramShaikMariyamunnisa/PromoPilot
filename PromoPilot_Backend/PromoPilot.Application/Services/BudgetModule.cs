using AutoMapper;
using PromoPilot.Application.DTOs;
using PromoPilot.Application.Interfaces;
using PromoPilot.Core.Entities;

namespace PromoPilot.Application.Services
{
    public class BudgetModule : IBudgetModule
    {
        private readonly IBudgetService _budgetService;
        private readonly IMapper _mapper;

        public BudgetModule(IBudgetService budgetService, IMapper mapper)
        {
            _budgetService = budgetService;
            _mapper = mapper;
        }

        public async Task<BudgetDto> CreateBudgetAsync(BudgetDto dto)
        {
            var entity = _mapper.Map<Budget>(dto);
            await _budgetService.AllocateBudgetAsync(entity);
            return _mapper.Map<BudgetDto>(entity);
        }

        public async Task<IEnumerable<BudgetDto>> GetAllBudgetsAsync()
        {
            var entities = await _budgetService.GetAllBudgetsAsync();
            return _mapper.Map<IEnumerable<BudgetDto>>(entities);
        }

        public async Task<BudgetDto> GetBudgetByIdAsync(int id)
        {
            var entity = await _budgetService.GetBudgetDetailsAsync(id);
            return _mapper.Map<BudgetDto>(entity);
        }

        public async Task<BudgetDto> UpdateBudgetAsync(int id, BudgetDto dto)
        {
            dto.BudgetID = id;
            return await _budgetService.UpdateAsync(dto);
        }

        public async Task<bool> DeleteBudgetAsync(int id)
        {
            var existing = await _budgetService.GetBudgetDetailsAsync(id);
            if (existing == null) return false;

            await _budgetService.RemoveBudgetAllocationAsync(id);
            return true;
        }
        public async Task<PagedResultDto<BudgetDto>> GetPagedBudgetsAsync(int pageNumber, int pageSize, string? sortBy, bool sortDesc)
        {
            var pagedEntities = await _budgetService.GetPagedAsync(pageNumber, pageSize, sortBy, sortDesc);
            var dtos = _mapper.Map<IEnumerable<BudgetDto>>(pagedEntities.Items);

            return new PagedResultDto<BudgetDto>
            {
                Items = dtos,
                TotalCount = pagedEntities.TotalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}