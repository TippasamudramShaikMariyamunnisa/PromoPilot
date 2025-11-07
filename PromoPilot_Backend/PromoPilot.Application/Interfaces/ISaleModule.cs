using PromoPilot.Application.DTOs;

namespace PromoPilot.Application.Interfaces
{
    public interface ISaleModule
    {
        Task<IEnumerable<SaleDto>> GetAllSalesAsync();
        Task<SaleDto> GetSaleByIdAsync(int id);
        Task<SaleDto> ProcessSaleAsync(SaleDto dto);
        Task<SaleDto> UpdateSaleAsync(int id, SaleDto dto);
        Task<bool> DeleteSaleAsync(int id);
        Task<byte[]> ExportSalesAsPdfAsync();
        Task<byte[]> ExportSalesAsCsvAsync();
        Task<PagedResultDto<SaleDto>> GetPagedSalesAsync(int pageNumber, int pageSize, string? sortBy, bool sortDesc);
    }
}
