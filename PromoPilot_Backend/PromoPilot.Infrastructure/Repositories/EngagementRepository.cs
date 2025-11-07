using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PromoPilot.Core.Interfaces;
using PromoPilot.Core.Entities;
using PromoPilot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace PromoPilot.Infrastructure.Repositories
{
    public class EngagementRepository : IEngagementRepository
    {
        private readonly PromoPilotDbContext _context;
        public EngagementRepository(PromoPilotDbContext context) => _context = context;

        public async Task<IEnumerable<Engagement>> GetAllAsync() => await _context.Engagements.ToListAsync();
        public async Task<Engagement> GetByIdAsync(int id)
        {
            var engagement = await _context.Engagements.FindAsync(id);
            if (engagement == null)
            {
                throw new KeyNotFoundException($"Engagement with ID {id} not found.");
            }
            return engagement;
        }
        public async Task AddAsync(Engagement engagement)
        {
            _context.Engagements.Add(engagement);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(Engagement engagement)
        {
            _context.Engagements.Update(engagement);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Engagements.FindAsync(id);
            if (entity != null)
            {
                _context.Engagements.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<bool> CampaignExistsAsync(int campaignId)
        {
            return await _context.Campaigns.AnyAsync(c => c.CampaignId == campaignId);
        }

        public async Task<bool> CustomerExistsAsync(int customerId)
        {
            return await _context.Customers.AnyAsync(c => c.CustomerId == customerId);
        }
        public IQueryable<Engagement> Query()
        {
            return _context.Engagements.AsQueryable();
        }

    }
}
