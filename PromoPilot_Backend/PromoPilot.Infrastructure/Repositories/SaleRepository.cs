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
    public class SaleRepository : ISaleRepository
    {
        private readonly PromoPilotDbContext _context;
        public SaleRepository(PromoPilotDbContext context) => _context = context;

        public async Task<IEnumerable<Sale>> GetAllAsync() => await _context.Sales.ToListAsync();
        public async Task<Sale> GetByIdAsync(int id)
        {
            var sale = await _context.Sales.FindAsync(id);
            if (sale == null)
            {
                throw new KeyNotFoundException($"Sale with ID {id} not found.");
            }
            return sale;
        }
        public async Task AddAsync(Sale sale)
        {
            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(Sale sale)
        {
            _context.Sales.Update(sale);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Sales.FindAsync(id);
            if (entity != null)
            {
                _context.Sales.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
        public IQueryable<Sale> Query()
        {
            return _context.Sales.AsQueryable();
        }
    }
}
