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
    public class BudgetRepository : IBudgetRepository
    {
        private readonly PromoPilotDbContext _context;
        public BudgetRepository(PromoPilotDbContext context) => _context = context;

        public async Task<IEnumerable<Budget>> GetAllAsync() => await _context.Budgets.ToListAsync();
        public async Task<Budget> GetByIdAsync(int id)
        {
            var budget = await _context.Budgets.FindAsync(id);
            if (budget == null)
            {
                throw new KeyNotFoundException($"Budget with ID {id} not found.");
            }
            return budget;
        }
        public async Task AddAsync(Budget budget)
        {
            _context.Budgets.Add(budget);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(Budget budget)
        {
            _context.Budgets.Update(budget);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Budgets.FindAsync(id);
            if (entity != null)
            {
                _context.Budgets.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
        public IQueryable<Budget> Query()
        {
            return _context.Budgets.AsQueryable();
        }
    }
}
