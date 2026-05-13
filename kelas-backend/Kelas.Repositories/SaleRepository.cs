using Kelas.Domain.Entities;
using Kelas.Domain.Interfaces.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Kelas.Repositories;

public class SaleRepository : ISaleRepository
{
    private readonly IMongoCollection<Sale> _collection;

    public SaleRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<Sale>("sales");
    }

    public async Task<Sale> CreateAsync(Sale entity, object? session = null)
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

    public async Task<List<Sale>> GetByFiltersAsync(DateTime? dateFrom, DateTime? dateTo, string? channel, string? paymentMethod)
    {
        var builder = Builders<Sale>.Filter;
        var filters = new List<FilterDefinition<Sale>>();

        if (dateFrom.HasValue)
            filters.Add(builder.Gte(x => x.Date, dateFrom.Value));

        if (dateTo.HasValue)
            filters.Add(builder.Lte(x => x.Date, dateTo.Value));

        if (!string.IsNullOrWhiteSpace(channel))
            filters.Add(builder.Eq(x => x.Channel, channel));

        if (!string.IsNullOrWhiteSpace(paymentMethod))
            filters.Add(builder.Eq(x => x.PaymentMethod, paymentMethod));

        var combinedFilter = filters.Any() ? builder.And(filters) : builder.Empty;

        return await _collection.Find(combinedFilter)
            .SortByDescending(x => x.Date)
            .ToListAsync();
    }

    public async Task<Sale?> GetByIdAsync(string id)
    {
        var objectId = ObjectId.Parse(id);
        return await _collection.Find(x => x.Id == objectId).FirstOrDefaultAsync();
    }
}
