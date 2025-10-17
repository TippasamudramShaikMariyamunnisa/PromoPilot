using PromoPilot.Application.DTOs;

namespace PromoPilot.Application.Interfaces
{
    public interface ICampaignReportModule
    {
        Task<IEnumerable<CampaignReportDto>> GetAllReportsAsync();
        Task<CampaignReportDto?> GetReportByIdAsync(int id);
        Task<CampaignReportDto?> GenerateReportAsync(int campaignId);
        Task<IEnumerable<CampaignReportWithRegionDto>> GetAllReportsWithRegionAsync();
        Task<IEnumerable<CampaignRegionPerformanceDto>> CompareByRegionAsync();
        Task<byte[]> ExportReportsAsPdfAsync();
        Task<byte[]> ExportReportsAsExcelAsync();
        Task<byte[]> ExportReportsAsXmlAsync();
        Task<PagedResultDto<CampaignReportDto>> GetPagedCampaignReportsAsync(int pageNumber, int pageSize, string? sortBy, bool sortDesc);
    }
}