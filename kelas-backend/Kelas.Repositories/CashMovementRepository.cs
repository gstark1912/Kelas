using Kelas.Domain.Entities;
using Kelas.Domain.Interfaces.Repositories;
using Kelas.Domain.Models.Requests;
using MongoDB.Bson;
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

    public async Task<List<CashMovement>> GetByFiltersAsync(CashMovementFilterRequest filter)
    {
        var builder = Builders<CashMovement>.Filter;
        var filters = new List<FilterDefinition<CashMovement>>();

        if (filter.DateFrom.HasValue)
            filters.Add(builder.Gte(x => x.Date, filter.DateFrom.Value));

        if (filter.DateTo.HasValue)
            filters.Add(builder.Lte(x => x.Date, filter.DateTo.Value));

        if (!string.IsNullOrWhiteSpace(filter.Type))
        {
            var type = filter.Type.Trim();
            var storedTypes = type.Equals("Ingreso", StringComparison.OrdinalIgnoreCase)
                ? new[] { "Ingreso", "income" }
                : new[] { "Egreso", "expense" };
            filters.Add(builder.In(x => x.Type, storedTypes));
        }

        if (!string.IsNullOrWhiteSpace(filter.Concept))
            filters.Add(builder.Eq(x => x.Concept, filter.Concept.Trim()));

        if (!string.IsNullOrWhiteSpace(filter.CashAccountId))
            filters.Add(builder.Eq(x => x.CashAccountId, ObjectId.Parse(filter.CashAccountId)));

        var combinedFilter = filters.Count > 0 ? builder.And(filters) : builder.Empty;

        return await _collection
            .Find(combinedFilter)
            .Sort(Builders<CashMovement>.Sort.Descending(x => x.Date).Descending(x => x.Id))
            .ToListAsync();
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
