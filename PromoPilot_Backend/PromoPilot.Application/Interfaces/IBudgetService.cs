using PromoPilot.Application.DTOs;
using PromoPilot.Core.Entities;

namespace PromoPilot.Application.Interfaces
{
    public interface IBudgetService
    {
        Task<IEnumerable<Budget>> GetAllBudgetsAsync();
        Task<Budget> GetBudgetDetailsAsync(int id);
        Task AllocateBudgetAsync(Budget budget);
        Task TrackBudgetSpendAsync(Budget budget);
        Task RemoveBudgetAllocationAsync(int id);
        Task<BudgetDto> UpdateAsync(BudgetDto dto);
        Task<PagedResultDto<Budget>> GetPagedAsync(int pageNumber, int pageSize, string? sortBy, bool sortDesc);
    }
}
