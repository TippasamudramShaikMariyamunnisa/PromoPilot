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
    public class ProductRepository : IProductRepository
    {
        private readonly PromoPilotDbContext _context;
        public ProductRepository(PromoPilotDbContext context) => _context = context;

        public async Task<IEnumerable<Product>> GetAllAsync() => await _context.Products.ToListAsync();
        public async Task<Product> GetByIdAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {id} not found.");
            }
            return product;
        }
        public async Task AddAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Products.FindAsync(id);
            if (entity != null)
            {
                _context.Products.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
        public IQueryable<Product> Query()
        {
            return _context.Products.AsQueryable();
        }
    }
}
