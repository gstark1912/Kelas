using Kelas.Domain.Entities;
using Kelas.Domain.Interfaces.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Kelas.Repositories;

public class RawMaterialRepository : IRawMaterialRepository
{
    private readonly IMongoCollection<RawMaterial> _collection;

    public RawMaterialRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<RawMaterial>("rawMaterials");
    }

    public async Task<List<RawMaterial>> GetActiveAsync(string? search = null, string? unit = null)
    {
        var builder = Builders<RawMaterial>.Filter;
        var filters = new List<FilterDefinition<RawMaterial>>
        {
            builder.Eq(x => x.IsActive, true)
        };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var escapedSearch = System.Text.RegularExpressions.Regex.Escape(search);
            filters.Add(builder.Regex(x => x.Name, new BsonRegularExpression(escapedSearch, "i")));
        }

        if (!string.IsNullOrWhiteSpace(unit))
        {
            filters.Add(builder.Eq(x => x.Unit, unit));
        }

        var combinedFilter = builder.And(filters);
        return await _collection.Find(combinedFilter).ToListAsync();
    }

    public async Task<RawMaterial?> GetByIdAsync(string id)
    {
        var objectId = ObjectId.Parse(id);
        return await _collection.Find(x => x.Id == objectId).FirstOrDefaultAsync();
    }

    public async Task<List<RawMaterial>> GetByIdsAsync(IEnumerable<ObjectId> ids)
    {
        var filter = Builders<RawMaterial>.Filter.In(x => x.Id, ids);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<RawMaterial?> GetByNameAsync(string name)
    {
        var escapedName = System.Text.RegularExpressions.Regex.Escape(name);
        var filter = Builders<RawMaterial>.Filter.And(
            Builders<RawMaterial>.Filter.Regex(
                x => x.Name,
                new BsonRegularExpression($"^{escapedName}$", "i")),
            Builders<RawMaterial>.Filter.Eq(x => x.IsActive, true));
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<RawMaterial> CreateAsync(RawMaterial entity, object? session = null)
    {
        entity.CreatedAt = DateTime.UtcNow;

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

    public async Task UpdateAsync(string id, RawMaterial entity)
    {
        var objectId = ObjectId.Parse(id);
        entity.UpdatedAt = DateTime.UtcNow;
        await _collection.ReplaceOneAsync(x => x.Id == objectId, entity);
    }

    public async Task EnsureIndexesAsync()
    {
        var indexModels = new List<CreateIndexModel<RawMaterial>>
        {
            new(
                Builders<RawMaterial>.IndexKeys.Ascending(x => x.Name),
                new CreateIndexOptions { Unique = true }),
            new(
                Builders<RawMaterial>.IndexKeys.Ascending(x => x.Unit)),
            new(
                Builders<RawMaterial>.IndexKeys.Ascending(x => x.IsActive))
        };

        await _collection.Indexes.CreateManyAsync(indexModels);
    }
}
