using Kelas.Domain.Entities;
using Kelas.Domain.Interfaces.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Kelas.Repositories;

public class ProductionBatchRepository : IProductionBatchRepository
{
    private readonly IMongoCollection<ProductionBatch> _collection;

    public ProductionBatchRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<ProductionBatch>("productionBatches");
    }

    public async Task<ProductionBatch> CreateAsync(ProductionBatch entity, object? session = null)
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

    public async Task<ProductionBatch?> GetByIdAsync(string id)
    {
        var objectId = ObjectId.Parse(id);
        return await _collection.Find(x => x.Id == objectId).FirstOrDefaultAsync();
    }

    public async Task<List<ProductionBatch>> GetAsync(string? productId = null, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var builder = Builders<ProductionBatch>.Filter;
        var filters = new List<FilterDefinition<ProductionBatch>>();

        if (!string.IsNullOrWhiteSpace(productId))
        {
            filters.Add(builder.Eq(x => x.ProductId, ObjectId.Parse(productId)));
        }

        if (dateFrom.HasValue)
        {
            filters.Add(builder.Gte(x => x.Date, dateFrom.Value));
        }

        if (dateTo.HasValue)
        {
            filters.Add(builder.Lte(x => x.Date, dateTo.Value));
        }

        var filter = filters.Any() ? builder.And(filters) : builder.Empty;
        return await _collection
            .Find(filter)
            .Sort(Builders<ProductionBatch>.Sort.Descending(x => x.Date))
            .ToListAsync();
    }

    public async Task EnsureIndexesAsync()
    {
        var indexModels = new List<CreateIndexModel<ProductionBatch>>
        {
            new(
                Builders<ProductionBatch>.IndexKeys
                    .Ascending(x => x.ProductId)
                    .Descending(x => x.Date)),
            new(
                Builders<ProductionBatch>.IndexKeys
                    .Descending(x => x.Date))
        };

        await _collection.Indexes.CreateManyAsync(indexModels);
    }
}
