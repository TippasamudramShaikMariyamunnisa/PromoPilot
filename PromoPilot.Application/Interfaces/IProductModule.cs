using PromoPilot.Application.DTOs;

namespace PromoPilot.Application.Interfaces
{
    public interface IProductModule
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<ProductDto> GetProductByIdAsync(int id);
        Task<ProductDto> AddProductAsync(ProductDto dto);
        Task<ProductDto> UpdateProductAsync(int id, ProductDto dto);
        Task<bool> DeleteProductAsync(int id);
        Task<byte[]> ExportProductsAsPdfAsync();
        Task<byte[]> ExportProductsAsCsvAsync();
        Task<PagedResultDto<ProductDto>> GetPagedProductsAsync(int pageNumber, int pageSize, string? sortBy, bool sortDesc);
    }
}