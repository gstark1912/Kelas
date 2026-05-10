using Kelas.Domain.Entities;
using Kelas.Domain.Interfaces.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Kelas.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly IMongoCollection<Product> _collection;

    public ProductRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<Product>("products");
    }

    public async Task<List<Product>> GetVisibleAsync(string? search = null)
    {
        var builder = Builders<Product>.Filter;
        var filters = new List<FilterDefinition<Product>>
        {
            builder.Eq(x => x.IsVisible, true)
        };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var escapedSearch = System.Text.RegularExpressions.Regex.Escape(search);
            filters.Add(builder.Regex(x => x.Name, new BsonRegularExpression(escapedSearch, "i")));
        }

        var combinedFilter = builder.And(filters);
        return await _collection.Find(combinedFilter).ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(string id)
    {
        var objectId = ObjectId.Parse(id);
        return await _collection.Find(x => x.Id == objectId).FirstOrDefaultAsync();
    }

    public async Task<Product?> GetByNameAsync(string name)
    {
        var escapedName = System.Text.RegularExpressions.Regex.Escape(name);
        var filter = Builders<Product>.Filter.Regex(
            x => x.Name,
            new BsonRegularExpression($"^{escapedName}$", "i"));
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<Product> CreateAsync(Product entity, object? session = null)
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

    public async Task UpdateAsync(string id, Product entity)
    {
        var objectId = ObjectId.Parse(id);
        entity.UpdatedAt = DateTime.UtcNow;
        await _collection.ReplaceOneAsync(x => x.Id == objectId, entity);
    }

    public async Task EnsureIndexesAsync()
    {
        var indexModels = new List<CreateIndexModel<Product>>
        {
            new(
                Builders<Product>.IndexKeys.Ascending(x => x.Name),
                new CreateIndexOptions { Unique = true }),
            new(
                Builders<Product>.IndexKeys.Ascending(x => x.IsVisible)),
            new(
                Builders<Product>.IndexKeys.Ascending("recipe.rawMaterialId"))
        };

        await _collection.Indexes.CreateManyAsync(indexModels);
    }
}
