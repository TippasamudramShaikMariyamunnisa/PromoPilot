using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PromoPilot.Core.Entities;

namespace PromoPilot.Core.Interfaces
{
    public interface ICampaignRepository
    {
        Task<IEnumerable<Campaign>> GetAllAsync();
        Task<Campaign> GetByIdAsync(int id);
        Task AddAsync(Campaign campaign);
        Task UpdateAsync(Campaign campaign);
        Task DeleteAsync(int id);
        Task<Campaign> GetByNameAndDatesAsync(string name, DateTime start, DateTime end);
        Task<bool> HasOverlappingCampaignAsync(DateTime startDate, DateTime endDate, List<string> storeList, List<string> productList);
        IQueryable<Campaign> Query();
    }
}
