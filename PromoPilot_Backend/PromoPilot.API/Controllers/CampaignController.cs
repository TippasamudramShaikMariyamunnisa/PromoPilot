using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PromoPilot.Application.DTOs;
using PromoPilot.Application.Interfaces;
using PromoPilot.Application.Services;
using PromoPilot.Infrastructure.Formatters;
using System.Text;


namespace PromoPilot.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json", "application/xml")]
    public class CampaignController : ControllerBase
    {
        private readonly ICampaignModule _campaignModule;
        private readonly ILogger<CampaignController> _logger;
        private readonly CampaignPdfGenerator _pdfGenerator;
        private readonly ICsvExporter _csvExporter;


        public CampaignController(ICampaignModule campaignModule, ILogger<CampaignController> logger, CampaignPdfGenerator pdfGenerator, ICsvExporter csvExporter)
        {
            _campaignModule = campaignModule;
            _logger = logger;
            _pdfGenerator = pdfGenerator;
            _csvExporter = csvExporter;
        }

        [Authorize(Roles = "Marketing")]
        [HttpPost("PlanCampaign")]
        public async Task<IActionResult> PlanCampaign([FromBody] CampaignDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                _logger.LogInformation("Planning new campaign: {Name}", dto.Name);
                var result = await _campaignModule.PlanCampaignAsync(dto);
                return CreatedAtAction(nameof(GetCampaignById), new { id = result.CampaignId }, result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business rule violation during campaign planning.");
                return Problem(
                    type: "https://promopilot.com/errors/business-rule",
                    title: "Campaign Planning Failed",
                    statusCode: StatusCodes.Status400BadRequest,
                    detail: ex.Message,
                    instance: HttpContext.Request.Path
                );
            }
        }

        [Authorize(Roles = "Marketing,StoreManager")]
        [HttpGet("GetAllCampaigns")]
        public async Task<IActionResult> GetAllCampaigns(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 5,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = false)
        {
            try
            {
                _logger.LogInformation("Fetching all campaigns.");
                var result = await _campaignModule.GetPagedCampaignsAsync(pageNumber, pageSize, sortBy, sortDesc);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching campaigns");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "Marketing,StoreManager")]
        [HttpGet("GetCampaignById/{id}")]
        public async Task<IActionResult> GetCampaignById(int id)
        {
            _logger.LogInformation("Fetching campaign with ID: {Id}", id);
            var result = await _campaignModule.GetCampaignByIdAsync(id);
            if (result == null)
            {
                _logger.LogWarning("Campaign with ID {Id} not found.", id);
                return Problem(
                    type: "https://promopilot.com/errors/not-found",
                    title: "Campaign Not Found",
                    statusCode: StatusCodes.Status404NotFound,
                    detail: $"No campaign found with ID {id}",
                    instance: HttpContext.Request.Path
                );
            }

            return Ok(result);
        }

        [Authorize(Roles = "Marketing")]
        [HttpPut("UpdateCampaign/{id}")]
        public async Task<IActionResult> UpdateCampaign(int id, [FromBody] CampaignDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            _logger.LogInformation("Updating campaign ID: {Id}", id);
            var result = await _campaignModule.UpdateCampaignAsync(id, dto);
            if (result == null)
            {
                _logger.LogWarning("Campaign with ID {Id} not found for update.", id);
                return Problem(
                    type: "https://promopilot.com/errors/not-found",
                    title: "Campaign Not Found",
                    statusCode: StatusCodes.Status404NotFound,
                    detail: $"No campaign found with ID {id} to update.",
                    instance: HttpContext.Request.Path
                );
            }

            return Ok(result);
        }

        [Authorize(Roles = "Marketing")]
        [HttpDelete("CancelCampaign/{id}")]
        public async Task<IActionResult> CancelCampaign(int id)
        {
            _logger.LogInformation("Canceling campaign ID: {Id}", id);
            var success = await _campaignModule.CancelCampaignAsync(id);
            if (!success)
            {
                _logger.LogWarning("Campaign with ID {Id} not found for cancellation.", id);
                return Problem(
                    type: "https://promopilot.com/errors/not-found",
                    title: "Campaign Not Found",
                    statusCode: StatusCodes.Status404NotFound,
                    detail: $"No campaign found with ID {id} to cancel.",
                    instance: HttpContext.Request.Path
                );
            }

            return NoContent();
        }

        [Authorize(Roles = "Marketing")]
        [HttpPut("ScheduleCampaign/{id}")]
        public async Task<IActionResult> ScheduleCampaign(int id)
        {
            using var reader = new StreamReader(Request.Body, Encoding.UTF8);
            var storeList = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(storeList))
            {
                var validationProblem = new ValidationProblemDetails(new Dictionary<string, string[]>
        {
            { "storeList", new[] { "Store list cannot be empty." } }
        })
                {
                    Type = "https://promopilot.com/errors/validation",
                    Title = "Validation Error",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = "One or more validation errors occurred.",
                    Instance = HttpContext.Request.Path
                };

                return ValidationProblem(validationProblem);
            }

            _logger.LogInformation("📥 Received storeList for campaign {Id}: {Stores}", id, storeList);

            var result = await _campaignModule.ScheduleCampaignAsync(id, storeList);
            if (result == null)
            {
                _logger.LogWarning("Campaign with ID {Id} not found for scheduling.", id);
                return Problem(
                    type: "https://promopilot.com/errors/not-found",
                    title: "Campaign Not Found",
                    statusCode: StatusCodes.Status404NotFound,
                    detail: $"No campaign found with ID {id} to schedule.",
                    instance: HttpContext.Request.Path
                );
            }

            return Ok(result);
        }




        [HttpGet("export/pdf")]
        [Produces("application/pdf")]
        public async Task<IActionResult> ExportCampaignsAsPdf()
        {
            var campaigns = await _campaignModule.GetAllCampaignsAsync();
            var pdfBytes = _pdfGenerator.Generate(campaigns);

            return File(pdfBytes, "application/pdf", "CampaignReport.pdf");
        }

        [HttpGet("export/csv")]
        [Produces("text/csv")]
        public async Task<IActionResult> ExportCampaignsAsCsv()
        {
            var campaigns = await _campaignModule.GetAllCampaignsAsync();
            return Ok(campaigns);// Will be formatted as CSV if Accept: text/csv
        }

    }
}