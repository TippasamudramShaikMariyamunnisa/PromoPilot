using PromoPilot.Application.DTOs;

namespace PromoPilot.Application.Interfaces
{
    public interface ICustomerModule
    {
        Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();
        Task<CustomerDto?> GetCustomerByIdAsync(int id);
        Task<CustomerDto> AddCustomerAsync(CustomerDto dto);
        Task<CustomerDto?> UpdateCustomerAsync(int id, CustomerDto dto);
        Task<bool> DeleteCustomerAsync(int id);
        Task<PagedResultDto<CustomerDto>> GetPagedCustomersAsync(int pageNumber, int pageSize, string? sortBy, bool sortDesc);
    }
}