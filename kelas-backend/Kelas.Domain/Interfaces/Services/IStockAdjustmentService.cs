using Kelas.Domain.Entities;
using Kelas.Domain.Models.Requests;
using Kelas.Domain.Models.Responses;
using MongoDB.Bson;

namespace Kelas.Domain.Interfaces.Services;

public interface IStockAdjustmentService
{
    Task<StockAdjustmentResponse> CreateAsync(CreateStockAdjustmentRequest request);
    Task<Stock?> GetStockByItemAsync(string itemType, string itemId);
    Task<List<Stock>> GetStockByItemsAsync(string itemType, IEnumerable<ObjectId> itemIds);
    Task<StockMovement> RegisterMovementAsync(
        string itemType,
        ObjectId itemId,
        string movementType,
        decimal quantity,
        DateTime date,
        string? referenceType = null,
        ObjectId? referenceId = null,
        string? adjustmentReason = null,
        string? notes = null,
        object? session = null);
    Task<List<StockMovementResponse>> GetMovementsByItemAsync(string itemType, string itemId);
}
