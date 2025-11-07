using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PromoPilot.Application.DTOs;
using PromoPilot.Core.Entities;

namespace PromoPilot.Application.Interfaces
{
    public interface IEngagementService
    {
        Task<IEnumerable<Engagement>> GetAllAsync();
        Task<Engagement> GetByIdAsync(int id);
        Task AddAsync(Engagement engagement);
        Task UpdateAsync(Engagement engagement);
        Task DeleteAsync(int id);
        Task<PagedResultDto<Engagement>> GetPagedAsync(int pageNumber, int pageSize, string? sortBy, bool sortDesc);
    }
}
