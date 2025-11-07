using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;
using AutoMapper;
using OfficeOpenXml;
using PromoPilot.Application.DTOs;
using PromoPilot.Application.Interfaces;

namespace PromoPilot.Application.Services
{
    public class CampaignReportModule : ICampaignReportModule
    {
        private readonly ICampaignReportService _reportService;
        private readonly IMapper _mapper;

        public CampaignReportModule(ICampaignReportService reportService, IMapper mapper)
        {
            _reportService = reportService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CampaignReportDto>> GetAllReportsAsync()
        {
            return await _reportService.GetAllReportsAsync();
        }

        public async Task<CampaignReportDto?> GetReportByIdAsync(int id)
        {
            return await _reportService.GetReportByIdAsync(id);
        }

        public async Task<CampaignReportDto?> GenerateReportAsync(int campaignId)
        {
            return await _reportService.GenerateReportAsync(campaignId);
        }

        public async Task<IEnumerable<CampaignReportWithRegionDto>> GetAllReportsWithRegionAsync()
        {
            return await _reportService.GetAllReportsWithRegionAsync();
        }

        public async Task<IEnumerable<CampaignRegionPerformanceDto>> CompareByRegionAsync()
        {
            return await _reportService.CompareByRegionAsync();
        }

        public async Task<byte[]> ExportReportsAsPdfAsync()
        {
            var reports = await _reportService.GetAllReportsAsync();
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream);
            writer.WriteLine("Campaign Reports");
            foreach (var r in reports)
            {
                writer.WriteLine($"ReportID: {r.ReportID}, CampaignID: {r.CampaignID}, ROI: {r.ROI}, Reach: {r.Reach}, ConversionRate: {r.ConversionRate}, GeneratedDate: {r.GeneratedDate}");
            }
            writer.Flush();
            return stream.ToArray();
        }

        public async Task<byte[]> ExportReportsAsExcelAsync()
        {
            var reports = await _reportService.GetAllReportsAsync();

            // Set EPPlus license context explicitly
            OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using var package = new OfficeOpenXml.ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Reports");
            worksheet.Cells["A1"].LoadFromCollection(reports, true);
            return package.GetAsByteArray();
        }

        public async Task<byte[]> ExportReportsAsXmlAsync()
        {
            var reports = (await _reportService.GetAllReportsAsync()).ToList();
            var xmlSerializer = new XmlSerializer(typeof(List<CampaignReportDto>));
            using var stringWriter = new StringWriter();
            xmlSerializer.Serialize(stringWriter, reports);
            return Encoding.UTF8.GetBytes(stringWriter.ToString());
        }
        public async Task<PagedResultDto<CampaignReportDto>> GetPagedCampaignReportsAsync(int pageNumber, int pageSize, string? sortBy, bool sortDesc)
        {
            var pagedEntities = await _reportService.GetPagedAsync(pageNumber, pageSize, sortBy, sortDesc);
            var dtos = _mapper.Map<IEnumerable<CampaignReportDto>>(pagedEntities.Items);

            return new PagedResultDto<CampaignReportDto>
            {
                Items = dtos,
                TotalCount = pagedEntities.TotalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}