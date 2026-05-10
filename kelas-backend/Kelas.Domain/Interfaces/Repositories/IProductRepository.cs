using Kelas.Domain.Entities;

namespace Kelas.Domain.Interfaces.Repositories;

public interface IProductRepository
{
    Task<List<Product>> GetVisibleAsync(string? search = null);
    Task<Product?> GetByIdAsync(string id);
    Task<Product?> GetByNameAsync(string name);
    Task<Product> CreateAsync(Product entity, object? session = null);
    Task UpdateAsync(string id, Product entity);
    Task EnsureIndexesAsync();
}
