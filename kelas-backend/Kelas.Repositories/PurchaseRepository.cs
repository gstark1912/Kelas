using Kelas.Domain.Entities;
using Kelas.Domain.Interfaces.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Kelas.Repositories;

public class PurchaseRepository : IPurchaseRepository
{
    private readonly IMongoCollection<Purchase> _collection;

    public PurchaseRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<Purchase>("purchases");
    }

    public async Task<Purchase> CreateAsync(Purchase entity, object? session = null)
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

    public async Task<List<Purchase>> GetByRawMaterialIdAsync(string rawMaterialId)
    {
        var objectId = ObjectId.Parse(rawMaterialId);
        var filter = Builders<Purchase>.Filter.Eq(x => x.RawMaterialId, objectId);
        var sort = Builders<Purchase>.Sort.Descending(x => x.Date);
        return await _collection.Find(filter).Sort(sort).ToListAsync();
    }

    public async Task EnsureIndexesAsync()
    {
        var indexModels = new List<CreateIndexModel<Purchase>>
        {
            new(
                Builders<Purchase>.IndexKeys
                    .Ascending(x => x.RawMaterialId)
                    .Descending(x => x.Date)),
            new(
                Builders<Purchase>.IndexKeys
                    .Descending(x => x.Date)),
            new(
                Builders<Purchase>.IndexKeys
                    .Ascending(x => x.CashAccountId))
        };

        await _collection.Indexes.CreateManyAsync(indexModels);
    }
}
