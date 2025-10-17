using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PromoPilot.Application.DTOs;
using PromoPilot.Application.Interfaces;
using PromoPilot.Application.Services;
using PromoPilot.Core.Entities;
using PromoPilot.Core.Interfaces;

namespace PromoPilot.Application.Services
{
    public class CampaignReportService : ICampaignReportService
    {
        private readonly ICampaignReportRepository _repository;
        private readonly IMapper _mapper;
        private readonly IAuditLoggingService _auditLoggingService;
        private readonly ILogger<CampaignReportService> _logger;

        public CampaignReportService(
            ICampaignReportRepository repository,
            IMapper mapper,
            IAuditLoggingService auditLoggingService,
            ILogger<CampaignReportService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _auditLoggingService = auditLoggingService;
            _logger = logger;
        }

        public async Task<IEnumerable<CampaignReportDto>> GetAllReportsAsync()
        {
            var reports = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<CampaignReportDto>>(reports);
        }

        public async Task<CampaignReportDto?> GetReportByIdAsync(int id)
        {
            var report = await _repository.GetByIdAsync(id);
            return report == null ? null : _mapper.Map<CampaignReportDto>(report);
        }

        public async Task<IEnumerable<CampaignReportWithRegionDto>> GetAllReportsWithRegionAsync()
        {
            var reports = await _repository.GetAllWithCampaignAsync();
            return reports.Select(r => new CampaignReportWithRegionDto
            {
                CampaignID = r.CampaignId,
                ROI = r.Roi,
                Reach = r.Reach,
                ConversionRate = r.ConversionRate,
                Region = r.Campaign.StoreList
            });
        }

        public async Task<CampaignReportDto?> GenerateReportAsync(int campaignId)
        {
            var campaign = await _repository.GetCampaignByIdAsync(campaignId);
            if (campaign == null)
                throw new KeyNotFoundException($"Campaign with ID {campaignId} not found.");

            decimal revenue = campaign.Sales.Sum(s => s.TotalAmount);
            decimal cost = campaign.Budgets.Sum(b => b.AllocatedAmount);
            int reach = campaign.Engagements.Count();
            int conversions = campaign.Sales.Count();

            decimal roi = cost == 0 ? 0 : ((revenue - cost) / cost) * 100;
            decimal conversionRate = reach == 0 ? 0 : ((decimal)conversions / reach) * 100;

            if (roi < 0)
            {
                _logger.LogWarning("Negative ROI detected for Campaign ID {CampaignId}: ROI = {ROI}", campaignId, roi);
            }

            _logger.LogInformation("Campaign ID: {CampaignId}", campaignId);
            _logger.LogInformation("Sales Count: {SalesCount}, Revenue: {Revenue}", campaign.Sales.Count(), revenue);
            _logger.LogInformation("Budget Count: {BudgetCount}, Cost: {Cost}", campaign.Budgets.Count(), cost);
            _logger.LogInformation("Engagement Count: {EngagementCount}", reach);
            _logger.LogInformation("Conversions: {Conversions}", conversions);
            _logger.LogInformation("ROI: {ROI}, Conversion Rate: {ConversionRate}", roi, conversionRate);


            if (conversionRate > 100)
                throw new InvalidOperationException("Conversion rate cannot exceed 100%.");

            if (conversions > reach)
                throw new InvalidOperationException("Conversions cannot exceed reach.");

            var report = new CampaignReport
            {
                CampaignId = campaignId,
                Roi = roi,
                Reach = reach,
                ConversionRate = conversionRate,
                GeneratedDate = DateTime.UtcNow
            };

            await _repository.AddAsync(report);

            await _auditLoggingService.LogAsync("Create", "CampaignReport", report.ReportId.ToString(), report);

            return _mapper.Map<CampaignReportDto>(report);
        }

        public async Task<IEnumerable<CampaignRegionPerformanceDto>> CompareByRegionAsync()
        {
            var reports = await _repository.GetAllWithCampaignAsync();
            var regionPerformanceList = new List<CampaignRegionPerformanceDto>();

            foreach (var report in reports)
            {
                var regions = report.Campaign.StoreList.Split(',').Select(r => r.Trim()).ToList();
                int regionCount = regions.Count;

                foreach (var region in regions)
                {
                    regionPerformanceList.Add(new CampaignRegionPerformanceDto
                    {
                        Region = region,
                        CampaignID = report.CampaignId,
                        TotalROI = report.Roi / regionCount,
                        TotalReach = report.Reach / regionCount,
                        AverageConversionRate = report.ConversionRate / regionCount
                    });
                }
            }

            return regionPerformanceList;
        }
        public async Task<PagedResultDto<CampaignReport>> GetPagedAsync(int pageNumber, int pageSize, string? sortBy, bool sortDesc)
        {
            if (pageNumber <= 0)
                throw new ArgumentException("Page number must be greater than zero.");

            if (pageSize <= 0 || pageSize > 100)
                throw new ArgumentException("Page size must be between 1 and 100.");

            var query = _repository.Query(); // IQueryable<CampaignReport>

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var reportProperties = typeof(CampaignReport).GetProperties().Select(p => p.Name).ToList();

                if (reportProperties.Contains(sortBy))
                {
                    query = sortDesc
                        ? query.OrderByDescending(e => EF.Property<object>(e, sortBy))
                        : query.OrderBy(e => EF.Property<object>(e, sortBy));
                }
                else
                {
                    throw new ArgumentException($"Invalid sort field: {sortBy}. Allowed fields are: {string.Join(", ", reportProperties)}");
                }
            }
            else
            {
                query = query.OrderBy(e => e.ReportId); // Default sort
            }

            var totalCount = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResultDto<CampaignReport>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}