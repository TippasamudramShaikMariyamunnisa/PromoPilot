using System.Linq;
using PromoPilot.Core.Entities;

namespace PromoPilot.Core.Interfaces
{
    public interface ICampaignReportRepository
    {
        Task<IEnumerable<CampaignReport>> GetAllAsync();
        Task<CampaignReport?> GetByIdAsync(int id);
        Task AddAsync(CampaignReport report);
        Task<IEnumerable<CampaignReport>> GetAllWithCampaignAsync();

        // ✅ Add this method
        Task<Campaign?> GetCampaignByIdAsync(int campaignId);
        IQueryable<CampaignReport> Query();
    }
}
