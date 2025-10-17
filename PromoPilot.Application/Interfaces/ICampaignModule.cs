using PromoPilot.Application.DTOs;

namespace PromoPilot.Application.Interfaces
{
    public interface ICampaignModule
    {
        Task<IEnumerable<CampaignDto>> GetAllCampaignsAsync();
        Task<CampaignDto> GetCampaignByIdAsync(int id);
        Task<CampaignDto> PlanCampaignAsync(CampaignDto dto);
        Task<CampaignDto> UpdateCampaignAsync(int id, CampaignDto dto);
        Task<bool> CancelCampaignAsync(int id);
        Task<CampaignDto> ScheduleCampaignAsync(int id, string storeList);
        Task<byte[]> ExportCampaignsAsPdfAsync();
        Task<byte[]> ExportCampaignsAsCsvAsync();
        Task<PagedResultDto<CampaignDto>> GetPagedCampaignsAsync(int pageNumber, int pageSize, string? sortBy, bool sortDesc);
    }
}