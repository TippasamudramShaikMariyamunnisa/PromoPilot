using AutoMapper;
using PromoPilot.Application.DTOs;
using PromoPilot.Application.Interfaces;
using PromoPilot.Application.Services;
using PromoPilot.Core.Entities;

public class SaleModule : ISaleModule
{
    private readonly ISaleService _saleService;
    private readonly IMapper _mapper;

    public SaleModule(ISaleService saleService, IMapper mapper)
    {
        _saleService = saleService;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SaleDto>> GetAllSalesAsync()
    {
        var entities = await _saleService.GetAllAsync();
        return _mapper.Map<IEnumerable<SaleDto>>(entities);
    }

    public async Task<SaleDto> GetSaleByIdAsync(int id)
    {
        var entity = await _saleService.GetByIdAsync(id);
        return _mapper.Map<SaleDto>(entity);
    }

    public async Task<SaleDto> ProcessSaleAsync(SaleDto dto)
    {
        var entity = _mapper.Map<Sale>(dto);
        await _saleService.AddAsync(entity);
        return _mapper.Map<SaleDto>(entity);
    }

    public async Task<SaleDto> UpdateSaleAsync(int id, SaleDto dto)
    {
        dto.SaleId = id;
        var entity = _mapper.Map<Sale>(dto);
        await _saleService.UpdateAsync(entity);
        return _mapper.Map<SaleDto>(entity);
    }

    public async Task<bool> DeleteSaleAsync(int id)
    {
        var existing = await _saleService.GetByIdAsync(id);
        if (existing == null) return false;

        await _saleService.DeleteAsync(id);
        return true;
    }

    public async Task<byte[]> ExportSalesAsPdfAsync()
    {
        var sales = await _saleService.GetAllAsync();
        // PDF generation logic placeholder
        return Array.Empty<byte>();
    }

    public async Task<byte[]> ExportSalesAsCsvAsync()
    {
        var sales = await _saleService.GetAllAsync();
        var csv = string.Join("\n", sales.Select(s => $"{s.SaleId},{s.CustomerId},{s.ProductId},{s.Quantity},{s.TotalAmount},{s.SaleDate}"));
        return System.Text.Encoding.UTF8.GetBytes(csv);
    }
    public async Task<PagedResultDto<SaleDto>> GetPagedSalesAsync(int pageNumber, int pageSize, string? sortBy, bool sortDesc)
    {
        var pagedEntities = await _saleService.GetPagedAsync(pageNumber, pageSize, sortBy, sortDesc);
        var dtos = _mapper.Map<IEnumerable<SaleDto>>(pagedEntities.Items);

        return new PagedResultDto<SaleDto>
        {
            Items = dtos,
            TotalCount = pagedEntities.TotalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}