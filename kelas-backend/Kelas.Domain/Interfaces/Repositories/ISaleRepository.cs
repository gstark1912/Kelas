using Kelas.Domain.Entities;
using MongoDB.Bson;

namespace Kelas.Domain.Interfaces.Repositories;

public interface ISaleRepository
{
    Task<Sale> CreateAsync(Sale entity, object? session = null);
    Task<List<Sale>> GetByFiltersAsync(DateTime? dateFrom, DateTime? dateTo, string? channel, string? paymentMethod);
    Task<Sale?> GetByIdAsync(string id);
}
