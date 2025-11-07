using PromoPilot.Application.DTOs;
using PromoPilot.Core.Entities;
namespace PromoPilot.Application.Interfaces;
public interface ICampaignService
{
    Task<IEnumerable<CampaignDto>> GetAllAsync();
    Task<CampaignDto> GetByIdAsync(int id);
    Task<CampaignDto> AddAsync(CampaignDto dto);
    Task<CampaignDto> UpdateAsync(int id, CampaignDto dto);
    Task DeleteAsync(int id);
    Task<PagedResultDto<Campaign>> GetPagedAsync(int pageNumber, int pageSize, string? sortBy, bool sortDesc);
    Task<CampaignDto> UpdateAsync(CampaignDto campaign);
    Task UpdateAsync(Campaign campaign);


}
