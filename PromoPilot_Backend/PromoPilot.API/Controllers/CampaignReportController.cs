using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using PromoPilot.Application.DTOs;
using PromoPilot.Application.Interfaces;
using PromoPilot.Infrastructure.Formatters;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;
using System.Text;
using System.Xml.Serialization;

namespace PromoPilot.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CampaignReportController : ControllerBase
    {
        private readonly ICampaignReportModule _reportModule;
        private readonly ILogger<CampaignReportController> _logger;
        private readonly CampaignPdfGenerator _pdfGenerator;
        private readonly IExcelExporter _excelExporter;

        public CampaignReportController(ICampaignReportModule reportModule, ILogger<CampaignReportController> logger, CampaignPdfGenerator pdfGenerator, IExcelExporter excelExporter)
        {
            _reportModule = reportModule;
            _logger = logger;
            _pdfGenerator = pdfGenerator;
            _excelExporter = excelExporter;
        }

        [Authorize(Roles = "Marketing")]
        [HttpGet("ViewAllCampaignReports")]
        public async Task<IActionResult> ViewAllCampaignReports(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 5,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool sortDesc = false)
        {
            _logger.LogInformation("Fetching all campaign reports with paging and sorting.");
            var result = await _reportModule.GetPagedCampaignReportsAsync(pageNumber, pageSize, sortBy, sortDesc);
            return Ok(result);
        }

        [Authorize(Roles = "Marketing")]
        [HttpGet("CompareByRegion")]
        public async Task<IActionResult> CompareByRegion()
        {
            _logger.LogInformation("Comparing campaign reports by region.");
            var result = await _reportModule.CompareByRegionAsync();
            return Ok(result);
        }

        [Authorize(Roles = "StoreManager")]
        [HttpPost("GenerateReport/{id}")]
        public async Task<IActionResult> GenerateReport(int id)
        {
            try
            {
                _logger.LogInformation("Generating report for campaign ID: {Id}", id);
                var report = await _reportModule.GenerateReportAsync(id);
                return Ok(report);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Campaign not found for report generation.");
                return Problem(
                    type: "https://promopilot.com/errors/not-found",
                    title: "Campaign Not Found",
                    statusCode: StatusCodes.Status404NotFound,
                    detail: ex.Message,
                    instance: HttpContext.Request.Path
                );
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business rule violation during report generation.");
                return Problem(
                    type: "https://promopilot.com/errors/business-rule",
                    title: "Report Generation Failed",
                    statusCode: StatusCodes.Status400BadRequest,
                    detail: ex.Message,
                    instance: HttpContext.Request.Path
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during report generation.");
                throw;
            }
        }

        [Authorize(Roles = "StoreManager")]
        [HttpGet("ExportReports")]
        public async Task<IActionResult> ExportReports([FromQuery] string format = "json")
        {
            var reports = await _reportModule.GetAllReportsAsync();

            switch (format.ToLower())
            {
                case "pdf":
                    var pdfBytes = GeneratePdf(reports);
                    return File(pdfBytes, "application/pdf", "campaign_reports.pdf");

                case "excel":
                    var excelBytes = GenerateExcel(reports);
                    return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "campaign_reports.xlsx");

                case "xml":
                    var xml = GenerateXml(reports);
                    return File(Encoding.UTF8.GetBytes(xml), "application/xml", "campaign_reports.xml");

                default:
                    return Ok(reports); // JSON
            }
        }
        //Helper Methods
        private byte[] GeneratePdf(IEnumerable<CampaignReportDto> reports)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Content().Column(col =>
                    {
                        col.Item().Text("Campaign Reports").Bold().FontSize(16).FontColor(Colors.Blue.Medium);

                        foreach (var r in reports)
                        {
                            col.Item().Text(
                                $"ReportID: {r.ReportID}, CampaignID: {r.CampaignID}, ROI: {r.ROI}, Reach: {r.Reach}, ConversionRate: {r.ConversionRate}, GeneratedDate: {r.GeneratedDate:yyyy-MM-dd}"
                            );
                        }
                    });
                });
            });

            return document.GeneratePdf();
        }
        private byte[] GenerateExcel(IEnumerable<CampaignReportDto> reports)
        {
            ExcelPackage.License.SetNonCommercialPersonal("Mariyamunnisa");
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Reports");
            worksheet.Cells["A1"].LoadFromCollection(reports, true);
            return package.GetAsByteArray();
        }
        private string GenerateXml(IEnumerable<CampaignReportDto> reports)
        {
            var xmlSerializer = new XmlSerializer(typeof(List<CampaignReportDto>));
            using var stringWriter = new StringWriter();
            xmlSerializer.Serialize(stringWriter, reports.ToList());
            return stringWriter.ToString();

        }
    }
}