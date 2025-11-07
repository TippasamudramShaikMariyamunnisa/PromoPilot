using AutoMapper;
using PromoPilot.Application.DTOs;
using PromoPilot.Application.Interfaces;
using PromoPilot.Core.Entities;
using PromoPilot.Core.Interfaces;

namespace PromoPilot.Application.Services
{
    public class CampaignModule : ICampaignModule
    {
        private readonly ICampaignService _campaignService;
        private readonly IMapper _mapper;


        public CampaignModule(ICampaignService campaignService, IMapper mapper)
        {
            _campaignService = campaignService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CampaignDto>> GetAllCampaignsAsync()
        {
            var entities = await _campaignService.GetAllAsync();
            return _mapper.Map<IEnumerable<CampaignDto>>(entities);
        }

        public async Task<CampaignDto> GetCampaignByIdAsync(int id)
        {
            var entity = await _campaignService.GetByIdAsync(id);
            return _mapper.Map<CampaignDto>(entity);
        }

        public async Task<CampaignDto> PlanCampaignAsync(CampaignDto dto)
        {
            var entity = _mapper.Map<Campaign>(dto);
            var result = await _campaignService.AddAsync(dto);
            return _mapper.Map<CampaignDto>(result);
        }

        public async Task<CampaignDto> UpdateCampaignAsync(int id, CampaignDto dto)
        {
            dto.CampaignId = id;
            var result = await _campaignService.UpdateAsync(id, dto);
            return _mapper.Map<CampaignDto>(result);
        }

        public async Task<bool> CancelCampaignAsync(int id)
        {
            var existing = await _campaignService.GetByIdAsync(id);
            if (existing == null) return false;

            await _campaignService.DeleteAsync(id);
            return true;
        }

        public async Task<CampaignDto> ScheduleCampaignAsync(int id, string storeList)
        {
            var campaign = await _campaignService.GetByIdAsync(id);
            if (campaign == null) return null;

            var existingStores = campaign.StoreList?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                    .Select(s => s.Trim())
                                                    .ToList() ?? new List<string>();

            var newStores = storeList.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                     .Select(s => s.Trim())
                                     .ToList();

            var mergedStores = existingStores.Union(newStores, StringComparer.OrdinalIgnoreCase);
            campaign.StoreList = string.Join(",", mergedStores);

            await _campaignService.UpdateAsync(campaign);
            return _mapper.Map<CampaignDto>(campaign);

        }


        public async Task<byte[]> ExportCampaignsAsPdfAsync()
        {
            var campaigns = await _campaignService.GetAllAsync();
            return Array.Empty<byte>(); // Placeholder
        }

        public async Task<byte[]> ExportCampaignsAsCsvAsync()
        {
            var campaigns = await _campaignService.GetAllAsync();
            var csv = string.Join("\n", campaigns.Select(c => $"{c.CampaignId},{c.Name},{c.StartDate},{c.EndDate}"));
            return System.Text.Encoding.UTF8.GetBytes(csv);
        }
        public async Task<PagedResultDto<CampaignDto>> GetPagedCampaignsAsync(int pageNumber, int pageSize, string? sortBy, bool sortDesc)
        {
            var pagedEntities = await _campaignService.GetPagedAsync(pageNumber, pageSize, sortBy, sortDesc);
            var dtos = _mapper.Map<IEnumerable<CampaignDto>>(pagedEntities.Items);

            return new PagedResultDto<CampaignDto>
            {
                Items = dtos,
                TotalCount = pagedEntities.TotalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}