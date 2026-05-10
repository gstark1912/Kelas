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
