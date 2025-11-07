using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PromoPilot.Core.Entities;
using PromoPilot.Core.Interfaces;
using PromoPilot.Infrastructure.Data;

namespace PromoPilot.Infrastructure.Repositories
{
    public class CampaignRepository : ICampaignRepository
    {
        private readonly PromoPilotDbContext _context;

        public CampaignRepository(PromoPilotDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Campaign>> GetAllAsync()
        {
            return await _context.Campaigns.ToListAsync();
        }

        public async Task<Campaign> GetByIdAsync(int id)
        {
            var campaign = await _context.Campaigns.FindAsync(id);
            if (campaign == null)
            {
                throw new KeyNotFoundException($"Campaign with ID {id} not found.");
            }
            return campaign;
        }

        public async Task AddAsync(Campaign campaign)
        {
            await _context.Campaigns.AddAsync(campaign);
            await _context.SaveChangesAsync();
        }
        public async Task<Campaign> GetByNameAndDatesAsync(string name, DateTime start, DateTime end)
        {
            return await _context.Campaigns
                .FirstOrDefaultAsync(c => c.Name == name && c.StartDate == start && c.EndDate == end);
        }


        public async Task UpdateAsync(Campaign entity)
        {
            _context.Campaigns.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Campaigns.FindAsync(id);
            if (entity != null)
            {
                _context.Campaigns.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<bool> HasOverlappingCampaignAsync(DateTime startDate, DateTime endDate, List<string> storeList, List<string> productList)
        {
            return await _context.Campaigns.AnyAsync(c =>
                c.StartDate <= endDate &&
                c.EndDate >= startDate &&
                storeList.Any(store => c.StoreList.Contains(store)) &&
                productList.Any(product => c.TargetProducts.Contains(product))
            );
        }
        public IQueryable<Campaign> Query()
        {
            return _context.Campaigns.AsQueryable();
        }

    }
}
