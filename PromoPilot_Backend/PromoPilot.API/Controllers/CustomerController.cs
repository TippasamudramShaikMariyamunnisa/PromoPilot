using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PromoPilot.Application.DTOs;
using PromoPilot.Application.Interfaces;

namespace PromoPilot.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerModule _customerModule;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(ICustomerModule customerModule, ILogger<CustomerController> logger)
        {
            _customerModule = customerModule;
            _logger = logger;
        }

        [Authorize(Roles = "Marketing")]
        [HttpGet("ViewAllCustomers")]
        public async Task<IActionResult> ViewAllCustomers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 5,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool sortDesc = false)
        {
            _logger.LogInformation("Fetching all customers.");
            var result = await _customerModule.GetPagedCustomersAsync(pageNumber, pageSize, sortBy, sortDesc);
            return Ok(result);
        }


        [Authorize(Roles = "Marketing")]
        [HttpGet("ViewCustomerById/{id}")]
        public async Task<IActionResult> ViewCustomerById(int id)
        {
            _logger.LogInformation("Fetching customer with ID: {Id}", id);
            var result = await _customerModule.GetCustomerByIdAsync(id);
            if (result == null)
            {
                _logger.LogWarning("Customer with ID {Id} not found.", id);
                return Problem(
                    type: "https://promopilot.com/errors/not-found",
                    title: "Customer Not Found",
                    statusCode: StatusCodes.Status404NotFound,
                    detail: $"No customer found with ID {id}",
                    instance: HttpContext.Request.Path
                );
            }

            return Ok(result);
        }

        [Authorize(Roles = "Marketing")]
        [HttpPost("AddCustomer")]
        public async Task<IActionResult> AddCustomer([FromBody] CustomerDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            try
            {
                _logger.LogInformation("Adding new customer.");
                var result = await _customerModule.AddCustomerAsync(dto);
                return CreatedAtAction(nameof(ViewCustomerById), new { id = result.CustomerID }, result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business rule violation during customer creation.");
                return Problem(
                    type: "https://promopilot.com/errors/business-rule",
                    title: "Customer Creation Failed",
                    statusCode: StatusCodes.Status400BadRequest,
                    detail: ex.Message,
                    instance: HttpContext.Request.Path
                );
            }
        }

        [Authorize(Roles = "Marketing")]
        [HttpPut("UpdateCustomer/{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] CustomerDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            _logger.LogInformation("Updating customer with ID: {Id}", id);
            var result = await _customerModule.UpdateCustomerAsync(id, dto);
            if (result == null)
            {
                _logger.LogWarning("Customer with ID {Id} not found for update.", id);
                return Problem(
                    type: "https://promopilot.com/errors/not-found",
                    title: "Customer Not Found",
                    statusCode: StatusCodes.Status404NotFound,
                    detail: $"No customer found with ID {id} to update.",
                    instance: HttpContext.Request.Path
                );
            }

            return Ok(result);
        }

        [Authorize(Roles = "Marketing")]
        [HttpDelete("DeleteCustomer/{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            _logger.LogInformation("Deleting customer with ID: {Id}", id);
            var success = await _customerModule.DeleteCustomerAsync(id);
            if (!success)
            {
                _logger.LogWarning("Customer with ID {Id} not found for deletion.", id);
                return Problem(
                    type: "https://promopilot.com/errors/not-found",
                    title: "Customer Not Found",
                    statusCode: StatusCodes.Status404NotFound,
                    detail: $"No customer found with ID {id} to delete.",
                    instance: HttpContext.Request.Path
                );
            }

            return NoContent();
        }
    }
}