using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PromoPilot.Application.DTOs;
using PromoPilot.Application.Interfaces;

namespace PromoPilot.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExecutionStatusController : ControllerBase
    {
        private readonly IExecutionStatusModule _executionStatusModule;
        private readonly ILogger<ExecutionStatusController> _logger;

        public ExecutionStatusController(IExecutionStatusModule executionStatusModule, ILogger<ExecutionStatusController> logger)
        {
            _executionStatusModule = executionStatusModule;
            _logger = logger;
        }

        [Authorize(Roles = "StoreManager")]
        [HttpGet("ViewAllExecutionStatuses")]
        public async Task<IActionResult> ViewAllExecutionStatuses(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 5,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool sortDesc = false)
        {
            _logger.LogInformation("Fetching all execution statuses.");
            var result = await _executionStatusModule.GetPagedExecutionStatusesAsync(pageNumber, pageSize, sortBy, sortDesc);
            return Ok(result);
        }

        [Authorize(Roles = "StoreManager,Marketing")]
        [HttpGet("ViewExecutionStatusById/{id}")]
        public async Task<IActionResult> ViewExecutionStatusById(int id)
        {
            _logger.LogInformation("Fetching execution status with ID: {Id}", id);
            var result = await _executionStatusModule.GetExecutionStatusByIdAsync(id);
            if (result == null)
            {
                _logger.LogWarning("Execution status with ID {Id} not found.", id);
                return Problem(
                    type: "https://promopilot.com/errors/not-found",
                    title: "Execution Status Not Found",
                    statusCode: StatusCodes.Status404NotFound,
                    detail: $"No execution status found with ID {id}",
                    instance: HttpContext.Request.Path
                );
            }

            return Ok(result);
        }

        [Authorize(Roles = "StoreManager")]
        [HttpPost("AddExecutionStatus")]
        public async Task<IActionResult> AddExecutionStatus([FromBody] ExecutionStatusDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            _logger.LogInformation("Adding new execution status.");
            var result = await _executionStatusModule.AddExecutionStatusAsync(dto);
            return CreatedAtAction(nameof(ViewExecutionStatusById), new { id = result.StatusID }, result);
        }

        [Authorize(Roles = "StoreManager")]
        [HttpPut("UpdateExecutionStatus/{id}")]
        public async Task<IActionResult> UpdateExecutionStatus(int id, [FromBody] ExecutionStatusDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            if (id != dto.StatusID)
            {
                _logger.LogWarning("ID mismatch between URL and body for execution status update.");
                return Problem(
                    type: "https://promopilot.com/errors/validation",
                    title: "Validation Error",
                    statusCode: StatusCodes.Status400BadRequest,
                    detail: "ID mismatch between URL and body.",
                    instance: HttpContext.Request.Path
                );
            }

            _logger.LogInformation("Updating execution status with ID: {Id}", id);
            var success = await _executionStatusModule.UpdateExecutionStatusAsync(dto);
            if (!success)
            {
                _logger.LogWarning("Execution status with ID {Id} not found for update.", id);
                return Problem(
                    type: "https://promopilot.com/errors/not-found",
                    title: "Execution Status Not Found",
                    statusCode: StatusCodes.Status404NotFound,
                    detail: $"No execution status found with ID {id} to update.",
                    instance: HttpContext.Request.Path
                );
            }

            var updatedStatus = await _executionStatusModule.GetExecutionStatusByIdAsync(id);
            return Ok(updatedStatus); 
        }

        [Authorize(Roles = "StoreManager")]
        [HttpDelete("DeleteExecutionStatus/{id}")]
        public async Task<IActionResult> DeleteExecutionStatus(int id)
        {
            _logger.LogInformation("Deleting execution status with ID: {Id}", id);
            var success = await _executionStatusModule.DeleteExecutionStatusAsync(id);
            if (!success)
            {
                _logger.LogWarning("Execution status with ID {Id} not found for deletion.", id);
                return Problem(
                    type: "https://promopilot.com/errors/not-found",
                    title: "Execution Status Not Found",
                    statusCode: StatusCodes.Status404NotFound,
                    detail: $"No execution status found with ID {id} to delete.",
                    instance: HttpContext.Request.Path
                );
            }

            return NoContent();
        }
    }
}