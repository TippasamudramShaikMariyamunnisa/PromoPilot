using AutoMapper;
using PromoPilot.Application.DTOs;
using PromoPilot.Application.Interfaces;
using PromoPilot.Core.Entities;

namespace PromoPilot.Application.Services
{
    public class ProductModule : IProductModule
    {
        private readonly IProductService _productService;
        private readonly IMapper _mapper;

        public ProductModule(IProductService productService, IMapper mapper)
        {
            _productService = productService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var entities = await _productService.GetAllAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(entities);
        }

        public async Task<ProductDto> GetProductByIdAsync(int id)
        {
            var entity = await _productService.GetByIdAsync(id);
            return _mapper.Map<ProductDto>(entity);
        }

        public async Task<ProductDto> AddProductAsync(ProductDto dto)
        {
            var entity = _mapper.Map<Product>(dto);
            await _productService.AddAsync(entity);
            return _mapper.Map<ProductDto>(entity);
        }

        public async Task<ProductDto> UpdateProductAsync(int id, ProductDto dto)
        {
            dto.ProductID = id;
            var entity = _mapper.Map<Product>(dto);
            await _productService.UpdateAsync(entity);
            return _mapper.Map<ProductDto>(entity);
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var existing = await _productService.GetByIdAsync(id);
            if (existing == null) return false;

            await _productService.DeleteAsync(id);
            return true;
        }

        public async Task<byte[]> ExportProductsAsPdfAsync()
        {
            var products = await _productService.GetAllAsync();
            // PDF generation logic placeholder
            return Array.Empty<byte>();
        }

        public async Task<byte[]> ExportProductsAsCsvAsync()
        {
            var products = await _productService.GetAllAsync();
            var csv = string.Join("\n", products.Select(p => $"{p.ProductId},{p.Name},{p.Price},{p.Category}"));
            return System.Text.Encoding.UTF8.GetBytes(csv);
        }
        public async Task<PagedResultDto<ProductDto>> GetPagedProductsAsync(int pageNumber, int pageSize, string? sortBy, bool sortDesc)
        {
            var pagedEntities = await _productService.GetPagedAsync(pageNumber, pageSize, sortBy, sortDesc);
            var dtos = _mapper.Map<IEnumerable<ProductDto>>(pagedEntities.Items);

            return new PagedResultDto<ProductDto>
            {
                Items = dtos,
                TotalCount = pagedEntities.TotalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}