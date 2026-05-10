using Kelas.Domain.Entities;

namespace Kelas.Domain.Interfaces.Repositories;

public interface IStockMovementRepository
{
    Task<StockMovement> CreateAsync(StockMovement entity, object? session = null);
    Task<List<StockMovement>> GetByItemAsync(string itemType, string itemId);
    Task EnsureIndexesAsync();
}
