using Kelas.Domain.Entities;
using MongoDB.Bson;

namespace Kelas.Domain.Interfaces.Repositories;

public interface IStockRepository
{
    Task<Stock?> GetByItemAsync(string itemType, string itemId);
    Task<List<Stock>> GetByItemsAsync(string itemType, IEnumerable<ObjectId> itemIds);
    Task<List<Stock>> GetByItemTypeAsync(string itemType);
    Task<Stock> CreateAsync(Stock entity, object? session = null);
    Task IncrementQuantityAsync(string itemType, string itemId, decimal quantity, object? session = null);
    Task UpsertIncrementQuantityAsync(string itemType, ObjectId itemId, decimal quantity, object? session = null);
    Task EnsureIndexesAsync();
}
