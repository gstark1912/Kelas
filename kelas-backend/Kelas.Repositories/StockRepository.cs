using Kelas.Domain.Entities;
using Kelas.Domain.Interfaces.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Kelas.Repositories;

public class StockRepository : IStockRepository
{
    private readonly IMongoCollection<Stock> _collection;

    public StockRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<Stock>("stock");
    }

    public async Task<Stock?> GetByItemAsync(string itemType, string itemId)
    {
        var objectId = ObjectId.Parse(itemId);
        var filter = Builders<Stock>.Filter.And(
            Builders<Stock>.Filter.Eq(x => x.ItemType, itemType),
            Builders<Stock>.Filter.Eq(x => x.ItemId, objectId));
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<List<Stock>> GetByItemsAsync(string itemType, IEnumerable<ObjectId> itemIds)
    {
        var ids = itemIds.Distinct().ToList();
        if (ids.Count == 0)
            return new List<Stock>();

        var filter = Builders<Stock>.Filter.And(
            Builders<Stock>.Filter.Eq(x => x.ItemType, itemType),
            Builders<Stock>.Filter.In(x => x.ItemId, ids));
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<List<Stock>> GetByItemTypeAsync(string itemType)
    {
        var filter = Builders<Stock>.Filter.Eq(x => x.ItemType, itemType);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<Stock> CreateAsync(Stock entity, object? session = null)
    {
        if (session is IClientSessionHandle clientSession)
        {
            await _collection.InsertOneAsync(clientSession, entity);
        }
        else
        {
            await _collection.InsertOneAsync(entity);
        }

        return entity;
    }

    public async Task IncrementQuantityAsync(string itemType, string itemId, decimal quantity, object? session = null)
    {
        var objectId = ObjectId.Parse(itemId);
        var filter = Builders<Stock>.Filter.And(
            Builders<Stock>.Filter.Eq(x => x.ItemType, itemType),
            Builders<Stock>.Filter.Eq(x => x.ItemId, objectId));
        var update = Builders<Stock>.Update
            .Inc(x => x.CurrentQuantity, quantity)
            .Set(x => x.LastUpdated, DateTime.UtcNow);

        if (session is IClientSessionHandle clientSession)
        {
            await _collection.UpdateOneAsync(clientSession, filter, update);
        }
        else
        {
            await _collection.UpdateOneAsync(filter, update);
        }
    }

    public async Task UpsertIncrementQuantityAsync(string itemType, ObjectId itemId, decimal quantity, object? session = null)
    {
        var filter = Builders<Stock>.Filter.And(
            Builders<Stock>.Filter.Eq(x => x.ItemType, itemType),
            Builders<Stock>.Filter.Eq(x => x.ItemId, itemId));
        var update = Builders<Stock>.Update
            .SetOnInsert(x => x.ItemType, itemType)
            .SetOnInsert(x => x.ItemId, itemId)
            .Inc(x => x.CurrentQuantity, quantity)
            .Set(x => x.LastUpdated, DateTime.UtcNow);
        var options = new UpdateOptions { IsUpsert = true };

        if (session is IClientSessionHandle clientSession)
        {
            await _collection.UpdateOneAsync(clientSession, filter, update, options);
        }
        else
        {
            await _collection.UpdateOneAsync(filter, update, options);
        }
    }

    public async Task EnsureIndexesAsync()
    {
        var indexModel = new CreateIndexModel<Stock>(
            Builders<Stock>.IndexKeys
                .Ascending(x => x.ItemType)
                .Ascending(x => x.ItemId),
            new CreateIndexOptions { Unique = true });

        await _collection.Indexes.CreateOneAsync(indexModel);
    }
}
