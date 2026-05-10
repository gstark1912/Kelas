using Kelas.Domain.Models.Requests;
using Kelas.Domain.Models.Responses;

namespace Kelas.Domain.Interfaces.Services;

public interface IStockAdjustmentService
{
    Task<StockAdjustmentResponse> CreateAsync(CreateStockAdjustmentRequest request);
    Task<List<StockMovementResponse>> GetMovementsByItemAsync(string itemType, string itemId);
}
