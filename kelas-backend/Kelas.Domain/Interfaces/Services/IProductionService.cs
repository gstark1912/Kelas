using Kelas.Domain.Models.Requests;
using Kelas.Domain.Models.Responses;

namespace Kelas.Domain.Interfaces.Services;

public interface IProductionService
{
    Task<ProductionBatchDetailResponse> CreateAsync(CreateProductionBatchRequest request);
    Task<ProductionBatchListResultResponse> GetAsync(string? productId = null, DateTime? dateFrom = null, DateTime? dateTo = null);
    Task<ProductionBatchDetailResponse> GetByIdAsync(string id);
}
