using AutoMapper;
using PromoPilot.Application.DTOs;
using PromoPilot.Application.Interfaces;
using PromoPilot.Core.Entities;

namespace PromoPilot.Application.Services
{
    public class CustomerModule : ICustomerModule
    {
        private readonly ICustomerService _customerService;
        private readonly IMapper _mapper;

        public CustomerModule(ICustomerService customerService, IMapper mapper)
        {
            _customerService = customerService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
        {
            var entities = await _customerService.GetAllAsync();
            return _mapper.Map<IEnumerable<CustomerDto>>(entities);
        }

        public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
        {
            var entity = await _customerService.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<CustomerDto>(entity);
        }

        public async Task<CustomerDto> AddCustomerAsync(CustomerDto dto)
        {
            var entity = _mapper.Map<Customer>(dto);
            await _customerService.AddAsync(entity);
            return _mapper.Map<CustomerDto>(entity);
        }

        public async Task<CustomerDto?> UpdateCustomerAsync(int id, CustomerDto dto)
        {
            var existing = await _customerService.GetByIdAsync(id);
            if (existing == null) return null;

            _mapper.Map(dto, existing);
            await _customerService.UpdateAsync(existing);
            return _mapper.Map<CustomerDto>(existing);
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            var existing = await _customerService.GetByIdAsync(id);
            if (existing == null) return false;

            await _customerService.DeleteAsync(id);
            return true;
        }
        public async Task<PagedResultDto<CustomerDto>> GetPagedCustomersAsync(int pageNumber, int pageSize, string? sortBy, bool sortDesc)
        {
            var pagedEntities = await _customerService.GetPagedAsync(pageNumber, pageSize, sortBy, sortDesc);
            var dtos = _mapper.Map<IEnumerable<CustomerDto>>(pagedEntities.Items);

            return new PagedResultDto<CustomerDto>
            {
                Items = dtos,
                TotalCount = pagedEntities.TotalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}