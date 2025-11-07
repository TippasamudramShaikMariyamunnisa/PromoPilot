using PromoPilot.Application.DTOs;

namespace PromoPilot.Application.Interfaces
{
    public interface IEngagementModule
    {
        Task<IEnumerable<EngagementDto>> GetAllEngagementsAsync();
        Task<EngagementDto?> GetEngagementByIdAsync(int id);
        Task<EngagementDto> TrackEngagementAsync(EngagementDto dto);
        Task<EngagementDto?> UpdateEngagementAsync(int id, EngagementDto dto);
        Task<bool> DeleteEngagementAsync(int id);
        Task<byte[]> ExportEngagementsAsCsvAsync();
        Task<byte[]> ExportEngagementsAsExcelAsync();
        Task<PagedResultDto<EngagementDto>> GetPagedEngagementsAsync(int pageNumber, int pageSize, string? sortBy, bool sortDesc);
    }
}
