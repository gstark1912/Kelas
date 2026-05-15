using Kelas.Domain.Entities;
using Kelas.Domain.Interfaces.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Kelas.Repositories;

public class CashAccountRepository : ICashAccountRepository
{
    private readonly IMongoCollection<CashAccount> _collection;

    public CashAccountRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<CashAccount>("cashAccounts");
    }

    public async Task<List<CashAccount>> GetActiveAsync()
    {
        return await _collection.Find(x => x.IsActive).ToListAsync();
    }

    public async Task<List<CashAccount>> GetByIdsAsync(IEnumerable<ObjectId> ids)
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0)
            return new List<CashAccount>();

        var filter = Builders<CashAccount>.Filter.In(x => x.Id, idList);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<CashAccount?> GetByIdAsync(string id)
    {
        var objectId = ObjectId.Parse(id);
        return await _collection.Find(x => x.Id == objectId).FirstOrDefaultAsync();
    }

    public async Task<CashAccount?> GetByNameAsync(string name)
    {
        var filter = Builders<CashAccount>.Filter.Regex(
            x => x.Name,
            new BsonRegularExpression($"^{System.Text.RegularExpressions.Regex.Escape(name)}$", "i"));
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<CashAccount> CreateAsync(CashAccount entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        await _collection.InsertOneAsync(entity);
        return entity;
    }

    public async Task DecrementBalanceAsync(string id, decimal amount, object? session = null)
    {
        var objectId = ObjectId.Parse(id);
        var filter = Builders<CashAccount>.Filter.Eq(x => x.Id, objectId);
        var update = Builders<CashAccount>.Update.Inc(x => x.CurrentBalance, -amount);

        if (session is IClientSessionHandle clientSession)
        {
            await _collection.UpdateOneAsync(clientSession, filter, update);
        }
        else
        {
            await _collection.UpdateOneAsync(filter, update);
        }
    }

    public async Task IncrementBalanceAsync(string id, decimal amount, object? session = null)
    {
        var objectId = ObjectId.Parse(id);
        var filter = Builders<CashAccount>.Filter.Eq(x => x.Id, objectId);
        var update = Builders<CashAccount>.Update.Inc(x => x.CurrentBalance, amount);

        if (session is IClientSessionHandle clientSession)
        {
            await _collection.UpdateOneAsync(clientSession, filter, update);
        }
        else
        {
            await _collection.UpdateOneAsync(filter, update);
        }
    }

    public async Task EnsureIndexesAsync()
    {
        var indexModels = new List<CreateIndexModel<CashAccount>>
        {
            new(
                Builders<CashAccount>.IndexKeys.Ascending(x => x.Name),
                new CreateIndexOptions { Unique = true }),
            new(
                Builders<CashAccount>.IndexKeys.Ascending(x => x.IsActive))
        };

        await _collection.Indexes.CreateManyAsync(indexModels);
    }
}
