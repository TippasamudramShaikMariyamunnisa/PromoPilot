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
    public class ProductController : ControllerBase
    {
        private readonly IProductModule _productModule;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductModule productModule, ILogger<ProductController> logger)
        {
            _productModule = productModule;
            _logger = logger;
        }

        [Authorize(Roles = "Marketing")]
        [HttpPost("AddProduct")]
        public async Task<IActionResult> AddProduct([FromBody] ProductDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            _logger.LogInformation("Adding new product: {Name}", dto.Name);
            var result = await _productModule.AddProductAsync(dto);
            return CreatedAtAction(nameof(GetProductById), new { id = result.ProductID }, result);
        }

        [Authorize(Roles = "Marketing")]
        [HttpGet("GetAllProducts")]
        public async Task<IActionResult> GetAllProducts(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 5,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool sortDesc = false)
        {
            _logger.LogInformation("Fetching all products.");
            var result = await _productModule.GetPagedProductsAsync(pageNumber, pageSize, sortBy, sortDesc);
            return Ok(result);
        }


        [Authorize(Roles = "Marketing")]
        [HttpGet("GetProductById/{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            _logger.LogInformation("Fetching product with ID: {Id}", id);
            var result = await _productModule.GetProductByIdAsync(id);
            if (result == null)
            {
                _logger.LogWarning("Product with ID {Id} not found.", id);
                return Problem(
                    type: "https://promopilot.com/errors/not-found",
                    title: "Product Not Found",
                    statusCode: StatusCodes.Status404NotFound,
                    detail: $"No product found with ID {id}",
                    instance: HttpContext.Request.Path
                );
            }

            return Ok(result);
        }

        [Authorize(Roles = "Marketing")]
        [HttpPut("UpdateProduct/{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            _logger.LogInformation("Updating product ID: {Id}", id);
            var result = await _productModule.UpdateProductAsync(id, dto);
            if (result == null)
            {
                _logger.LogWarning("Product with ID {Id} not found for update.", id);
                return Problem(
                    type: "https://promopilot.com/errors/not-found",
                    title: "Product Not Found",
                    statusCode: StatusCodes.Status404NotFound,
                    detail: $"No product found with ID {id} to update.",
                    instance: HttpContext.Request.Path
                );
            }

            return Ok(result);
        }

        [Authorize(Roles = "Marketing")]
        [HttpDelete("DeleteProduct/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            _logger.LogInformation("Deleting product ID: {Id}", id);
            var success = await _productModule.DeleteProductAsync(id);
            if (!success)
            {
                _logger.LogWarning("Product with ID {Id} not found for deletion.", id);
                return Problem(
                    type: "https://promopilot.com/errors/not-found",
                    title: "Product Not Found",
                    statusCode: StatusCodes.Status404NotFound,
                    detail: $"No product found with ID {id} to delete.",
                    instance: HttpContext.Request.Path
                );
            }

            return NoContent();
        }
    }
}