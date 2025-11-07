using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PromoPilot.Core.Entities;

namespace PromoPilot.Core.Interfaces
{
    public interface IEngagementRepository
    {
        Task<IEnumerable<Engagement>> GetAllAsync();
        Task<Engagement> GetByIdAsync(int id);
        Task AddAsync(Engagement engagement);
        Task UpdateAsync(Engagement engagement);
        Task DeleteAsync(int id);
        Task<bool> CampaignExistsAsync(int campaignId);
        Task<bool> CustomerExistsAsync(int customerId);
        IQueryable<Engagement> Query();
    }
}
