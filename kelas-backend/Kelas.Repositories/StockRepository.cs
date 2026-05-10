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
