using PromoPilot.Application.DTOs;

namespace PromoPilot.Application.Interfaces
{
    public interface IExecutionStatusModule
    {
        Task<IEnumerable<ExecutionStatusDto>> GetAllExecutionStatusesAsync();
        Task<ExecutionStatusDto?> GetExecutionStatusByIdAsync(int id);
        Task<ExecutionStatusDto> AddExecutionStatusAsync(ExecutionStatusDto dto);
        Task<bool> UpdateExecutionStatusAsync(ExecutionStatusDto dto);
        Task<bool> DeleteExecutionStatusAsync(int id);
        Task<PagedResultDto<ExecutionStatusDto>> GetPagedExecutionStatusesAsync(int pageNumber, int pageSize, string? sortBy, bool sortDesc);
    }
}