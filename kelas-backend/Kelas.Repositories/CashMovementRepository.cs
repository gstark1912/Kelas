using Kelas.Domain.Entities;
using Kelas.Domain.Interfaces.Repositories;
using MongoDB.Driver;

namespace Kelas.Repositories;

public class CashMovementRepository : ICashMovementRepository
{
    private readonly IMongoCollection<CashMovement> _collection;

    public CashMovementRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<CashMovement>("cashMovements");
    }

    public async Task<CashMovement> CreateAsync(CashMovement entity, object? session = null)
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
        var indexModels = new List<CreateIndexModel<CashMovement>>
        {
            new(
                Builders<CashMovement>.IndexKeys
                    .Ascending(x => x.CashAccountId)
                    .Descending(x => x.Date)),
            new(
                Builders<CashMovement>.IndexKeys
                    .Descending(x => x.Date)),
            new(
                Builders<CashMovement>.IndexKeys
                    .Ascending(x => x.ReferenceType)
                    .Ascending(x => x.ReferenceId))
        };

        await _collection.Indexes.CreateManyAsync(indexModels);
    }
}
