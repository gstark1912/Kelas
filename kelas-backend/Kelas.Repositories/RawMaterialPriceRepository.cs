using Kelas.Domain.Entities;
using Kelas.Domain.Interfaces.Repositories;
using MongoDB.Driver;

namespace Kelas.Repositories;

public class RawMaterialPriceRepository : IRawMaterialPriceRepository
{
    private readonly IMongoCollection<RawMaterialPrice> _collection;

    public RawMaterialPriceRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<RawMaterialPrice>("rawMaterialPrices");
    }

    public async Task<RawMaterialPrice> CreateAsync(RawMaterialPrice entity, object? session = null)
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
        var indexModel = new CreateIndexModel<RawMaterialPrice>(
            Builders<RawMaterialPrice>.IndexKeys
                .Ascending(x => x.RawMaterialId)
                .Descending(x => x.DateFrom));

        await _collection.Indexes.CreateOneAsync(indexModel);
    }
}
