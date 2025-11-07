using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PromoPilot.Application.DTOs;
using PromoPilot.Application.Interfaces;

namespace PromoPilot.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json", "application/xml", "text/csv", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    public class BudgetController : ControllerBase
    {
        private readonly IBudgetModule _budgetModule;
        private readonly IMapper _mapper;
        private readonly ILogger<BudgetController> _logger;
        private readonly IBudgetService _budgetService;
        private readonly ICsvExporter _csvExporter;
        private readonly IExcelExporter _excelExporter;

        public BudgetController(
            IBudgetModule budgetModule,
            IMapper mapper,
            ILogger<BudgetController> logger,
            IBudgetService budgetService,
            ICsvExporter csvExporter,
            IExcelExporter excelExporter)
        {
            _budgetModule = budgetModule;
            _mapper = mapper;
            _logger = logger;
            _budgetService = budgetService;
            _csvExporter = csvExporter;
            _excelExporter = excelExporter;
        }

        [HttpGet("ViewAllBudgets")]
        [Authorize(Roles = "Finance")]

        public async Task<IActionResult> ViewAllBudgets(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 5,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool sortDesc = false)

        {

            _logger.LogInformation("Fetching all budgets.");
            var result = await _budgetModule.GetPagedBudgetsAsync(pageNumber, pageSize, sortBy, sortDesc);
            return Ok(result);
        }

        [HttpGet("ViewBudgetById/{id}")]
        [Authorize(Roles = "Finance,Marketing")]
        public async Task<IActionResult> ViewBudgetById(int id)
        {
            _logger.LogInformation("Fetching budget with ID: {Id}", id);
            var entity = await _budgetModule.GetBudgetByIdAsync(id);
            if (entity == null)
            {
                _logger.LogWarning("Budget with ID {Id} not found.", id);
                return Problem(
                    type: "https://promopilot.com/errors/not-found",
                    title: "Budget Not Found",
                    statusCode: StatusCodes.Status404NotFound,
                    detail: $"No budget found with ID {id}",
                    instance: HttpContext.Request.Path
                );
            }

            var dto = _mapper.Map<BudgetDto>(entity);
            return Ok(dto);
        }

        [HttpPost("AllocateBudget")]
        [Authorize(Roles = "Finance")]
        public async Task<IActionResult> AllocateBudget([FromBody] BudgetDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for budget allocation.");
                return ValidationProblem(ModelState);
            }

            try
            {
                _logger.LogInformation("Allocating new budget.");
                var resultDto = await _budgetModule.CreateBudgetAsync(dto);
                return CreatedAtAction(nameof(ViewBudgetById), new { id = resultDto.BudgetID }, resultDto);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business rule violation during budget allocation.");
                return Problem(
                    type: "https://promopilot.com/errors/business-rule",
                    title: "Budget Allocation Failed",
                    statusCode: StatusCodes.Status400BadRequest,
                    detail: ex.Message,
                    instance: HttpContext.Request.Path
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during budget allocation.");
                throw;
            }
        }

        [HttpPut("UpdateBudget/{id}")]
        [Authorize(Roles = "Finance")]
        public async Task<IActionResult> UpdateBudget(int id, [FromBody] BudgetDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for budget update.");
                return ValidationProblem(ModelState);
            }

            try
            {
                _logger.LogInformation("Updating budget with ID: {Id}", id);
                var resultDto = await _budgetModule.UpdateBudgetAsync(id, dto);
                if (resultDto == null)
                {
                    _logger.LogWarning("Budget with ID {Id} not found for update.", id);
                    return Problem(
                        type: "https://promopilot.com/errors/not-found",
                        title: "Budget Not Found",
                        statusCode: StatusCodes.Status404NotFound,
                        detail: $"No budget found with ID {id} to update.",
                        instance: HttpContext.Request.Path
                    );
                }

                return Ok(resultDto);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business rule violation during budget update.");
                return Problem(
                    type: "https://promopilot.com/errors/business-rule",
                    title: "Budget Update Failed",
                    statusCode: StatusCodes.Status400BadRequest,
                    detail: ex.Message,
                    instance: HttpContext.Request.Path
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during budget update.");
                throw;
            }
        }

        [HttpDelete("DeleteBudget/{id}")]
        [Authorize(Roles = "Finance")]
        public async Task<IActionResult> DeleteBudget(int id)
        {
            _logger.LogInformation("Deleting budget with ID: {Id}", id);
            var deleted = await _budgetModule.DeleteBudgetAsync(id);
            if (!deleted)
            {
                _logger.LogWarning("Budget with ID {Id} not found for deletion.", id);
                return Problem(
                    type: "https://promopilot.com/errors/not-found",
                    title: "Budget Not Found",
                    statusCode: StatusCodes.Status404NotFound,
                    detail: $"No budget found with ID {id} to delete.",
                    instance: HttpContext.Request.Path
                );
            }

            return NoContent();
        }
        [HttpGet("export/csv")]
        public async Task<IActionResult> ExportCsv()
        {
            var budgets = await _budgetService.GetAllBudgetsAsync();
            var budgetDtos = _mapper.Map<IEnumerable<BudgetDto>>(budgets);
            var csv = _csvExporter.ExportBudgetsToCsv(budgetDtos);
            var bytes = Encoding.UTF8.GetBytes(csv);
            return File(bytes, "text/csv", "Budgets.csv");
        }

        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportExcel()
        {
            var budgets = await _budgetService.GetAllBudgetsAsync();
            var budgetDtos = _mapper.Map<IEnumerable<BudgetDto>>(budgets);
            var fileBytes = _excelExporter.ExportBudgetsToExcel(budgetDtos);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Budgets.xlsx");
        }
    }
}