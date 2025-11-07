using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using PromoPilot.Application.DTOs;
using PromoPilot.Application.Interfaces;

namespace PromoPilot.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EngagementController : ControllerBase
    {
        private readonly IEngagementModule _engagementModule;
        private readonly ILogger<EngagementController> _logger;
        private readonly IMapper _mapper;

        public EngagementController(IEngagementModule engagementModule, ILogger<EngagementController> logger, IMapper mapper)
        {
            _engagementModule = engagementModule;
            _logger = logger;
            _mapper = mapper;
        }

        [Authorize(Roles = "Marketing, StoreManager")]
        [HttpGet("ViewAllEngagements")]
        public async Task<IActionResult> ViewAllEngagements(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool sortDesc = false)
        {
            _logger.LogInformation("Fetching all engagements.");
            var result = await _engagementModule.GetPagedEngagementsAsync(pageNumber, pageSize, sortBy, sortDesc);
            return Ok(result);
        }


        [Authorize(Roles = "StoreManager")]
        [HttpGet("ViewEngagementById/{id}")]
        public async Task<IActionResult> ViewEngagementById(int id)
        {
            _logger.LogInformation("Fetching engagement with ID: {Id}", id);
            var result = await _engagementModule.GetEngagementByIdAsync(id);
            if (result == null)
            {
                _logger.LogWarning("Engagement with ID {Id} not found.", id);
                return Problem(
                    type: "https://promopilot.com/errors/not-found",
                    title: "Engagement Not Found",
                    statusCode: StatusCodes.Status404NotFound,
                    detail: $"No engagement found with ID {id}",
                    instance: HttpContext.Request.Path
                );
            }

            return Ok(result);
        }

        [Authorize(Roles = "StoreManager")]
        [HttpPost("TrackEngagement")]
        public async Task<IActionResult> TrackEngagement([FromBody] EngagementDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                _logger.LogInformation("Tracking new engagement.");
                var result = await _engagementModule.TrackEngagementAsync(dto);
                return CreatedAtAction(nameof(ViewEngagementById), new { id = result.EngagementID }, result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business rule violation during engagement tracking.");
                return Problem(
                    type: "https://promopilot.com/errors/business-rule",
                    title: "Engagement Tracking Failed",
                    statusCode: StatusCodes.Status400BadRequest,
                    detail: ex.Message,
                    instance: HttpContext.Request.Path
                );
            }
        }

        [Authorize(Roles = "StoreManager")]
        [HttpPut("UpdateEngagement/{id}")]
        public async Task<IActionResult> UpdateEngagement(int id, [FromBody] EngagementDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            _logger.LogInformation("Updating engagement with ID: {Id}", id);
            var result = await _engagementModule.UpdateEngagementAsync(id, dto);
            if (result == null)
            {
                _logger.LogWarning("Engagement with ID {Id} not found for update.", id);
                return Problem(
                    type: "https://promopilot.com/errors/not-found",
                    title: "Engagement Not Found",
                    statusCode: StatusCodes.Status404NotFound,
                    detail: $"No engagement found with ID {id} to update.",
                    instance: HttpContext.Request.Path
                );
            }

            return Ok(result);
        }

        [Authorize(Roles = "StoreManager")]
        [HttpDelete("DeleteEngagement/{id}")]
        public async Task<IActionResult> DeleteEngagement(int id)
        {
            _logger.LogInformation("Deleting engagement with ID: {Id}", id);
            var success = await _engagementModule.DeleteEngagementAsync(id);
            if (!success)
            {
                _logger.LogWarning("Engagement with ID {Id} not found for deletion.", id);
                return Problem(
                    type: "https://promopilot.com/errors/not-found",
                    title: "Engagement Not Found",
                    statusCode: StatusCodes.Status404NotFound,
                    detail: $"No engagement found with ID {id} to delete.",
                    instance: HttpContext.Request.Path
                );
            }

            return NoContent();
        }

        [HttpGet("ExportEngagements")]
        public async Task<IActionResult> ExportEngagements([FromQuery] string format = "json")
        {
            _logger.LogInformation("Exporting engagements in format: {Format}", format);
            var entities = await _engagementModule.GetAllEngagementsAsync();
            var dtos = _mapper.Map<IEnumerable<EngagementDto>>(entities);

            switch (format.ToLower())
            {
                case "csv":
                    var csv = GenerateCsv(dtos);
                    return File(Encoding.UTF8.GetBytes(csv), "text/csv", "engagements.csv");

                case "excel":
                    var excelBytes = GenerateExcel(dtos);
                    return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "engagements.xlsx");

                default:
                    return Ok(dtos); // JSON
            }
        }
        // Helper Methods
        private string GenerateCsv(IEnumerable<EngagementDto> dtos)
        {
            var sb = new StringBuilder();
            sb.AppendLine("EngagementID,CampaignID,CustomerID,RedemptionCount,PurchaseValue");

            foreach (var dto in dtos)
            {
                sb.AppendLine($"{dto.EngagementID},{dto.CampaignID},{dto.CustomerID},{dto.RedemptionCount},{dto.PurchaseValue}");
            }

            return sb.ToString();
        }

        private byte[] GenerateExcel(IEnumerable<EngagementDto> dtos)
        {
            ExcelPackage.License.SetNonCommercialPersonal("Mariyamunnisa");

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Engagements");

            worksheet.Cells["A1"].LoadFromCollection(dtos, true);
            return package.GetAsByteArray();
        }
    }
}