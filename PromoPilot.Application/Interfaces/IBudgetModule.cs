using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PromoPilot.Application.DTOs;

namespace PromoPilot.Application.Interfaces
{
    public interface IBudgetModule
    {
        Task<BudgetDto> CreateBudgetAsync(BudgetDto dto);
        Task<IEnumerable<BudgetDto>> GetAllBudgetsAsync();
        Task<BudgetDto> GetBudgetByIdAsync(int id);
        Task<BudgetDto> UpdateBudgetAsync(int id, BudgetDto dto);
        Task<bool> DeleteBudgetAsync(int id);
        Task<PagedResultDto<BudgetDto>> GetPagedBudgetsAsync(int pageNumber, int pageSize, string? sortBy, bool sortDesc);
    }
}