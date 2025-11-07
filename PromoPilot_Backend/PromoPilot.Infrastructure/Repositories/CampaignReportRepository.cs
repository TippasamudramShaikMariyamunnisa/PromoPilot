using Microsoft.EntityFrameworkCore;
using PromoPilot.Core.Entities;
using PromoPilot.Core.Interfaces;
using PromoPilot.Infrastructure.Data;

namespace PromoPilot.Infrastructure.Repositories
{
    public class CampaignReportRepository : ICampaignReportRepository
    {
        private readonly PromoPilotDbContext _context;

        public CampaignReportRepository(PromoPilotDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CampaignReport>> GetAllAsync()
        {
            return await _context.CampaignReports.ToListAsync();
        }

        public async Task<CampaignReport?> GetByIdAsync(int id)
        {
            return await _context.CampaignReports.FindAsync(id);
        }

        public async Task<Campaign?> GetCampaignByIdAsync(int id)
        {
            return await _context.Campaigns
                .Include(c => c.Sales)
                .Include(c => c.Budgets)
                .Include(c => c.Engagements)
                .FirstOrDefaultAsync(c => c.CampaignId == id);
        }

        public async Task AddAsync(CampaignReport report)
        {
            await _context.CampaignReports.AddAsync(report);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<CampaignReport>> GetAllWithCampaignAsync()
        {
            return await _context.CampaignReports.Include(r => r.Campaign).ToListAsync();
        }

        public IQueryable<CampaignReport> Query()
        {
            return _context.CampaignReports.AsQueryable();
        }
    }
}
