using AutoMapper;
using PromoPilot.Application.DTOs;
using PromoPilot.Application.Interfaces;
using PromoPilot.Core.Entities;

namespace PromoPilot.Application.Services
{
    public class ExecutionStatusModule : IExecutionStatusModule
    {
        private readonly IExecutionStatusService _executionStatusService;
        private readonly IMapper _mapper;

        public ExecutionStatusModule(IExecutionStatusService executionStatusService, IMapper mapper)
        {
            _executionStatusService = executionStatusService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ExecutionStatusDto>> GetAllExecutionStatusesAsync()
        {
            var entities = await _executionStatusService.GetAllAsync();
            return _mapper.Map<IEnumerable<ExecutionStatusDto>>(entities);
        }

        public async Task<ExecutionStatusDto?> GetExecutionStatusByIdAsync(int id)
        {
            var entity = await _executionStatusService.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<ExecutionStatusDto>(entity);
        }

        public async Task<ExecutionStatusDto> AddExecutionStatusAsync(ExecutionStatusDto dto)
        {
            var entity = _mapper.Map<ExecutionStatus>(dto);
            await _executionStatusService.AddAsync(entity);
            return _mapper.Map<ExecutionStatusDto>(entity);
        }

        public async Task<bool> UpdateExecutionStatusAsync(ExecutionStatusDto dto)
        {
            var existing = await _executionStatusService.GetByIdAsync(dto.StatusID);
            if (existing == null) return false;

            _mapper.Map(dto, existing);
            await _executionStatusService.UpdateAsync(existing);
            return true;
        }

        public async Task<bool> DeleteExecutionStatusAsync(int id)
        {
            var existing = await _executionStatusService.GetByIdAsync(id);
            if (existing == null) return false;

            await _executionStatusService.DeleteAsync(id);
            return true;
        }
        public async Task<PagedResultDto<ExecutionStatusDto>> GetPagedExecutionStatusesAsync(int pageNumber, int pageSize, string? sortBy, bool sortDesc)
        {
            var pagedEntities = await _executionStatusService.GetPagedAsync(pageNumber, pageSize, sortBy, sortDesc);
            var dtos = _mapper.Map<IEnumerable<ExecutionStatusDto>>(pagedEntities.Items);

            return new PagedResultDto<ExecutionStatusDto>
            {
                Items = dtos,
                TotalCount = pagedEntities.TotalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}