using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PromoPilot.Application.DTOs;
using PromoPilot.Application.Services;
using PromoPilot.Core.Entities;
using PromoPilot.Core.Interfaces;
using PromoPilot.Application.Interfaces;

namespace PromoPilot.Application.Services
{
    public class CampaignService : ICampaignService
    {
        private readonly ICampaignRepository _repository;
        private readonly IMapper _mapper;
        private readonly IAuditLoggingService _auditLoggingService;

        public CampaignService(
            ICampaignRepository repository,
            IMapper mapper,
            IAuditLoggingService auditLoggingService)
        {
            _repository = repository;
            _mapper = mapper;
            _auditLoggingService = auditLoggingService;
        }

        public async Task<IEnumerable<CampaignDto>> GetAllAsync()
        {
            var campaigns = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<CampaignDto>>(campaigns);
        }

        public async Task<CampaignDto> GetByIdAsync(int id)
        {
            var campaign = await _repository.GetByIdAsync(id);
            return _mapper.Map<CampaignDto>(campaign);
        }

        public async Task<CampaignDto> AddAsync(CampaignDto dto)
        {
            var hasOverlap = await _repository.HasOverlappingCampaignAsync(
                dto.StartDate,
                dto.EndDate,
                dto.StoreList.Split(',').ToList(),
                dto.TargetProducts.Split(',').ToList()
            );

            if (hasOverlap)
                throw new InvalidOperationException("Overlapping campaign exists for the selected products and stores.");

            var existing = await _repository.GetByNameAndDatesAsync(dto.Name, dto.StartDate, dto.EndDate);
            if (existing != null)
                throw new InvalidOperationException("Campaign already exists with same name and dates.");

            var campaign = _mapper.Map<Campaign>(dto);
            await _repository.AddAsync(campaign);

            await _auditLoggingService.LogAsync("Create", "Campaign", campaign.CampaignId.ToString(), dto);

            return _mapper.Map<CampaignDto>(campaign);
        }

        public async Task<CampaignDto> UpdateAsync(CampaignDto dto)
        {
            var existing = await _repository.GetByIdAsync(dto.CampaignId);
            if (existing == null) return null;

            _mapper.Map(dto, existing);
            await _repository.UpdateAsync(existing);

            await _auditLoggingService.LogAsync("Update", "Campaign", dto.CampaignId.ToString(), dto);

            return _mapper.Map<CampaignDto>(existing);
        }
        public async Task<CampaignDto> UpdateAsync(int id, CampaignDto dto)
        {
            dto.CampaignId = id;
            return await UpdateAsync(dto);
        }
        public async Task UpdateAsync(Campaign campaign)
        {
            await _repository.UpdateAsync(campaign);

            await _auditLoggingService.LogAsync("Update", "Campaign", campaign.CampaignId.ToString(), new
            {
                campaign.Name,
                campaign.StartDate,
                campaign.EndDate,
                campaign.StoreList,
                campaign.TargetProducts
            });
        }



        public async Task DeleteAsync(int id)
        {
            await _repository.DeleteAsync(id);

            await _auditLoggingService.LogAsync("Delete", "Campaign", id.ToString(), new { Deleted = true });
        }

        public async Task<PagedResultDto<Campaign>> GetPagedAsync(int pageNumber, int pageSize, string? sortBy, bool sortDesc)
        {
            if (pageNumber <= 0)
                throw new ArgumentException("Page number must be greater than zero.");

            if (pageSize <= 0 || pageSize > 100)
                throw new ArgumentException("Page size must be between 1 and 100.");

            var query = _repository.Query(); // IQueryable<Campaign>

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                // ✅ Normalize: capitalize first letter to match property names
                sortBy = char.ToUpper(sortBy[0]) + sortBy.Substring(1);

                var campaignProperties = typeof(Campaign).GetProperties().Select(p => p.Name).ToList();

                if (campaignProperties.Contains(sortBy))
                {
                    query = sortDesc
                        ? query.OrderByDescending(e => EF.Property<object>(e, sortBy))
                        : query.OrderBy(e => EF.Property<object>(e, sortBy));
                }
                else
                {
                    throw new ArgumentException($"Invalid sort field: {sortBy}. Allowed fields are: {string.Join(", ", campaignProperties)}");
                }
            }
            else
            {
                query = query.OrderBy(e => e.CampaignId); // Default sort
            }

            var totalCount = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResultDto<Campaign>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}