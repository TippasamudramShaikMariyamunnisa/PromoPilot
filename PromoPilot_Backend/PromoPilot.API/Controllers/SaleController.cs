using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PromoPilot.Application.DTOs;
using PromoPilot.Application.Interfaces;
using PromoPilot.Infrastructure.Formatters;
 
namespace PromoPilot.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json", "application/xml")]
    public class SalesController : ControllerBase
    {
        private readonly ISaleModule _saleModule;
        private readonly ILogger<SalesController> _logger;
        private readonly ICsvExporter _csvExporter;
        private readonly CampaignPdfGenerator _pdfGenerator;

        public SalesController(ISaleModule saleModule, ILogger<SalesController> logger)
        {
            _saleModule = saleModule;
            _logger = logger;
        }

        [Authorize(Roles = "StoreManager")]
        [HttpPost("ProcessSale")]
        public async Task<IActionResult> ProcessSale([FromBody] SaleDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                _logger.LogInformation("Processing new sale.");
                var result = await _saleModule.ProcessSaleAsync(dto);
                return CreatedAtAction(nameof(ViewSaleById), new { id = result.SaleId }, result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business rule violation during sale processing.");
                return Problem(
                    type: "https://promopilot.com/errors/business-rule",
                    title: "Sale Processing Failed",
                    statusCode: StatusCodes.Status400BadRequest,
                    detail: ex.Message,
                    instance: HttpContext.Request.Path
                );
            }
        }

        [Authorize(Roles = "Marketing,Finance")]
        [HttpGet("ViewAllSales")]
        public async Task<IActionResult> ViewAllSales(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 5,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool sortDesc = false)
        {
            _logger.LogInformation("Fetching all sales.");
            var result = await _saleModule.GetPagedSalesAsync(pageNumber, pageSize, sortBy, sortDesc);
            return Ok(result);
        }

        [Authorize(Roles = "Marketing,Finance")]
        [HttpGet("ViewSaleById/{id}")]
        public async Task<IActionResult> ViewSaleById(int id)
        {
            _logger.LogInformation("Fetching sale with ID: {Id}", id);
            var result = await _saleModule.GetSaleByIdAsync(id);
            if (result == null)
            {
                _logger.LogWarning("Sale with ID {Id} not found.", id);
                return Problem(
                    type: "https://promopilot.com/errors/not-found",
                    title: "Sale Not Found",
                    statusCode: StatusCodes.Status404NotFound,
                    detail: $"No sale found with ID {id}",
                    instance: HttpContext.Request.Path
                );
            }

            return Ok(result);
        }

        [Authorize(Roles = "StoreManager")]
        [HttpPut("UpdateSale/{id}")]
        public async Task<IActionResult> UpdateSale(int id, [FromBody] SaleDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            _logger.LogInformation("Updating sale with ID: {Id}", id);
            var result = await _saleModule.UpdateSaleAsync(id, dto);
            if (result == null)
            {
                _logger.LogWarning("Sale with ID {Id} not found for update.", id);
                return Problem(
                    type: "https://promopilot.com/errors/not-found",
                    title: "Sale Not Found",
                    statusCode: StatusCodes.Status404NotFound,
                    detail: $"No sale found with ID {id} to update.",
                    instance: HttpContext.Request.Path
                );
            }

            return Ok(result);
        }

        [Authorize(Roles = "StoreManager")]
        [HttpDelete("DeleteSale/{id}")]
        public async Task<IActionResult> DeleteSale(int id)
        {
            _logger.LogInformation("Deleting sale with ID: {Id}", id);
            var success = await _saleModule.DeleteSaleAsync(id);
            if (!success)
            {
                _logger.LogWarning("Sale with ID {Id} not found for deletion.", id);
                return Problem(
                    type: "https://promopilot.com/errors/not-found",
                    title: "Sale Not Found",
                    statusCode: StatusCodes.Status404NotFound,
                    detail: $"No sale found with ID {id} to delete.",
                    instance: HttpContext.Request.Path
                );
            }

            return NoContent();
        }
    }
}