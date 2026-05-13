using Kelas.Domain.Models.Requests;
using Kelas.Domain.Models.Responses;

namespace Kelas.Domain.Interfaces.Services;

public interface ISaleService
{
    Task<SaleResponse> CreateAsync(CreateSaleRequest request);
    Task<List<SaleResponse>> GetByFiltersAsync(DateTime? dateFrom, DateTime? dateTo, string? channel, string? paymentMethod);
    Task<SaleResponse> GetByIdAsync(string id);
}
