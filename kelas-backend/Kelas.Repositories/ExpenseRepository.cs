using Kelas.Domain.Entities;
using Kelas.Domain.Interfaces.Repositories;
using Kelas.Domain.Models.Requests;
using Kelas.Domain.Models.Responses;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Kelas.Repositories;

public class ExpenseRepository : IExpenseRepository
{
    private readonly IMongoCollection<Expense> _collection;

    public ExpenseRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<Expense>("expenses");
    }

    public async Task<Expense?> GetByIdAsync(string id)
    {
        var objectId = ObjectId.Parse(id);
        return await _collection.Find(x => x.Id == objectId).FirstOrDefaultAsync();
    }

    public async Task<List<Expense>> GetByFiltersAsync(ExpenseFilterRequest filter)
    {
        var builder = Builders<Expense>.Filter;
        var filters = new List<FilterDefinition<Expense>>();

        if (!string.IsNullOrEmpty(filter.Category))
            filters.Add(builder.Eq(x => x.Category, filter.Category));

        if (!string.IsNullOrEmpty(filter.CashAccountId))
            filters.Add(builder.Eq(x => x.CashAccountId, ObjectId.Parse(filter.CashAccountId)));

        if (filter.DateFrom.HasValue)
            filters.Add(builder.Gte(x => x.Date, filter.DateFrom.Value));

        if (filter.DateTo.HasValue)
            filters.Add(builder.Lte(x => x.Date, filter.DateTo.Value));

        var combinedFilter = filters.Any() ? builder.And(filters) : builder.Empty;

        return await _collection
            .Find(combinedFilter)
            .Sort(Builders<Expense>.Sort.Descending(x => x.Date))
            .ToListAsync();
    }

    public async Task<List<CategoryTotalResponse>> GetExpensesByCategoryAsync(DateTime from, DateTime to)
    {
        var pipeline = new[]
        {
            new BsonDocument("$match", new BsonDocument
            {
                { "date", new BsonDocument { { "$gte", from }, { "$lte", to } } }
            }),
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", "$category" },
                { "total", new BsonDocument("$sum", "$amount") },
                { "count", new BsonDocument("$sum", 1) }
            }),
            new BsonDocument("$sort", new BsonDocument("total", -1)),
            new BsonDocument("$project", new BsonDocument
            {
                { "_id", 0 },
                { nameof(CategoryTotalResponse.Category), "$_id" },
                { nameof(CategoryTotalResponse.Total), "$total" },
                { nameof(CategoryTotalResponse.Count), "$count" }
            })
        };

        return await _collection.Aggregate<CategoryTotalResponse>(pipeline).ToListAsync();
    }

    public async Task<Expense> CreateAsync(Expense entity, object? session = null)
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
}
