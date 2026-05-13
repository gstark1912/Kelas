using Kelas.Domain.Entities;
using MongoDB.Bson;

namespace Kelas.Domain.Interfaces.Repositories;

public interface IProductRepository
{
    Task<List<Product>> GetVisibleAsync(string? search = null);
    Task<List<Product>> GetByIdsAsync(IEnumerable<ObjectId> ids);
    Task<Product?> GetByIdAsync(string id);
    Task<Product?> GetByNameAsync(string name);
    Task<Product> CreateAsync(Product entity, object? session = null);
    Task UpdateAsync(string id, Product entity);
    Task EnsureIndexesAsync();
}
