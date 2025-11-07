using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PromoPilot.Core.Entities;

namespace PromoPilot.Core.Interfaces
{
    public interface IExecutionStatusRepository
    {
        Task<IEnumerable<ExecutionStatus>> GetAllAsync();
        Task<ExecutionStatus> GetByIdAsync(int id);
        Task AddAsync(ExecutionStatus status);
        Task UpdateAsync(ExecutionStatus status);
        Task DeleteAsync(int id);
        Task<bool> CampaignExistsAsync(int campaignId);
        IQueryable<ExecutionStatus> Query();

    }
}
