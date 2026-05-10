using Kelas.Domain.Entities;
using Kelas.Domain.Interfaces.Repositories;
using MongoDB.Driver;

namespace Kelas.Repositories;

public class StockMovementRepository : IStockMovementRepository
{
    private readonly IMongoCollection<StockMovement> _collection;

    public StockMovementRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<StockMovement>("stockMovements");
    }

    public async Task<StockMovement> CreateAsync(StockMovement entity, object? session = null)
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
        var indexModels = new List<CreateIndexModel<StockMovement>>
        {
            new(
                Builders<StockMovement>.IndexKeys
                    .Ascending(x => x.ItemType)
                    .Ascending(x => x.ItemId)
                    .Descending(x => x.Date)),
            new(
                Builders<StockMovement>.IndexKeys
                    .Ascending(x => x.ReferenceType)
                    .Ascending(x => x.ReferenceId)),
            new(
                Builders<StockMovement>.IndexKeys
                    .Descending(x => x.Date))
        };

        await _collection.Indexes.CreateManyAsync(indexModels);
    }
}
