using Kelas.Domain.Entities;

namespace Kelas.Domain.Interfaces.Repositories;

public interface IStockRepository
{
    Task<Stock?> GetByItemAsync(string itemType, string itemId);
    Task<List<Stock>> GetByItemTypeAsync(string itemType);
    Task<Stock> CreateAsync(Stock entity, object? session = null);
    Task EnsureIndexesAsync();
}
