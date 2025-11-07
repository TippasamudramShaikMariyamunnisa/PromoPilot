using PromoPilot.Application.DTOs;
using PromoPilot.Core.Entities;

namespace PromoPilot.Application.Interfaces
{
    public interface ICampaignReportService
    {
        Task<IEnumerable<CampaignReportDto>> GetAllReportsAsync();
        Task<CampaignReportDto?> GetReportByIdAsync(int id);
        Task<CampaignReportDto?> GenerateReportAsync(int campaignId);
        Task<IEnumerable<CampaignReportWithRegionDto>> GetAllReportsWithRegionAsync();
        Task<IEnumerable<CampaignRegionPerformanceDto>> CompareByRegionAsync(); // ✅ Add this
        Task<PagedResultDto<CampaignReport>> GetPagedAsync(int pageNumber, int pageSize, string? sortBy, bool sortDesc);
    }
}
