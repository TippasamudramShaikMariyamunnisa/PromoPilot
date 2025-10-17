using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PromoPilot.Core.Entities;
using PromoPilot.Application.DTOs;

namespace PromoPilot.Application.Interfaces
{
    public interface ICustomerService
    {
        Task<IEnumerable<Customer>> GetAllAsync();
        Task<Customer> GetByIdAsync(int id);
        Task AddAsync(Customer customer);
        Task UpdateAsync(Customer customer);
        Task DeleteAsync(int id);
        Task<PagedResultDto<Customer>> GetPagedAsync(int pageNumber, int pageSize, string? sortBy, bool sortDesc);
    }
}
