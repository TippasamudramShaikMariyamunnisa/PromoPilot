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
    public class ExecutionStatusRepository : IExecutionStatusRepository
    {
        private readonly PromoPilotDbContext _context;
        public ExecutionStatusRepository(PromoPilotDbContext context) => _context = context;

        public async Task<IEnumerable<ExecutionStatus>> GetAllAsync() => await _context.ExecutionStatuses.ToListAsync();
        public async Task<ExecutionStatus> GetByIdAsync(int id)
        {
            var status = await _context.ExecutionStatuses.FindAsync(id);
            if (status == null)
            {
                throw new KeyNotFoundException($"ExecutionStatus with ID {id} not found.");
            }
            return status;

        }
        public async Task AddAsync(ExecutionStatus status)
        {
            _context.ExecutionStatuses.Add(status);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(ExecutionStatus status)
        {
            _context.ExecutionStatuses.Update(status);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id)
        {
            var entity = await _context.ExecutionStatuses.FindAsync(id);
            if (entity != null)
            {
                _context.ExecutionStatuses.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<bool> CampaignExistsAsync(int campaignId)
        {
            return await _context.Campaigns.AnyAsync(c => c.CampaignId == campaignId);
        }
        public IQueryable<ExecutionStatus> Query()
        {
            return _context.ExecutionStatuses.AsQueryable();
        }
    }
}
