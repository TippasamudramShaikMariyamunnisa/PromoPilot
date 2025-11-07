using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PromoPilot.Application.Interfaces;
using PromoPilot.Application.DTOs;

namespace PromoPilot.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CampaignAnalyticsController : ControllerBase
    {
        private readonly ICampaignReportModule _reportModule;
        private readonly ILogger<CampaignAnalyticsController> _logger;

        public CampaignAnalyticsController(ICampaignReportModule reportModule, ILogger<CampaignAnalyticsController> logger)
        {
            _reportModule = reportModule;
            _logger = logger;
        }

        [Authorize(Roles = "Marketing")]
        [HttpGet("RoiSummary")]
        public async Task<IActionResult> GetRoiSummary()
        {
            _logger.LogInformation("Fetching ROI summary for all campaigns.");
            var reports = await _reportModule.GetAllReportsAsync();

            var roiSummary = reports.Select(r => new
            {
                CampaignID = r.CampaignID,
                ROI = r.ROI
            });

            return Ok(roiSummary);
        }
    }
}
