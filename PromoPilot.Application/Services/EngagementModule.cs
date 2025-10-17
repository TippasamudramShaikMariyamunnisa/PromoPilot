using AutoMapper;
using PromoPilot.Application.DTOs;
using PromoPilot.Application.Interfaces;
using PromoPilot.Core.Entities;
using OfficeOpenXml;
using System.Text;

namespace PromoPilot.Application.Services
{
    public class EngagementModule : IEngagementModule
    {
        private readonly IEngagementService _engagementService;
        private readonly IMapper _mapper;

        public EngagementModule(IEngagementService engagementService, IMapper mapper)
        {
            _engagementService = engagementService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<EngagementDto>> GetAllEngagementsAsync()
        {
            var entities = await _engagementService.GetAllAsync();
            return _mapper.Map<IEnumerable<EngagementDto>>(entities);
        }

        public async Task<EngagementDto?> GetEngagementByIdAsync(int id)
        {
            var entity = await _engagementService.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<EngagementDto>(entity);
        }

        public async Task<EngagementDto> TrackEngagementAsync(EngagementDto dto)
        {
            var entity = _mapper.Map<Engagement>(dto);
            await _engagementService.AddAsync(entity);
            return _mapper.Map<EngagementDto>(entity);
        }

        public async Task<EngagementDto?> UpdateEngagementAsync(int id, EngagementDto dto)
        {
            var existing = await _engagementService.GetByIdAsync(id);
            if (existing == null) return null;

            _mapper.Map(dto, existing);
            await _engagementService.UpdateAsync(existing);
            return _mapper.Map<EngagementDto>(existing);
        }

        public async Task<bool> DeleteEngagementAsync(int id)
        {
            var existing = await _engagementService.GetByIdAsync(id);
            if (existing == null) return false;

            await _engagementService.DeleteAsync(id);
            return true;
        }

        public async Task<byte[]> ExportEngagementsAsCsvAsync()
        {
            var engagements = await _engagementService.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<EngagementDto>>(engagements);

            var sb = new StringBuilder();
            sb.AppendLine("EngagementID,CampaignID,CustomerID,RedemptionCount,PurchaseValue");
            foreach (var dto in dtos)
            {
                sb.AppendLine($"{dto.EngagementID},{dto.CampaignID},{dto.CustomerID},{dto.RedemptionCount},{dto.PurchaseValue}");
            }
            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public async Task<byte[]> ExportEngagementsAsExcelAsync()
        {
            var engagements = await _engagementService.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<EngagementDto>>(engagements);

            OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Engagements");
            worksheet.Cells["A1"].LoadFromCollection(dtos, true);
            return package.GetAsByteArray();
        }
        public async Task<PagedResultDto<EngagementDto>> GetPagedEngagementsAsync(int pageNumber, int pageSize, string? sortBy, bool sortDesc)
        {
            var pagedEntities = await _engagementService.GetPagedAsync(pageNumber, pageSize, sortBy, sortDesc);
            var dtos = _mapper.Map<IEnumerable<EngagementDto>>(pagedEntities.Items);

            return new PagedResultDto<EngagementDto>
            {
                Items = dtos,
                TotalCount = pagedEntities.TotalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}