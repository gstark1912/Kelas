using Kelas.Domain.Entities;

namespace Kelas.Domain.Interfaces.Repositories;

public interface IPurchaseRepository
{
    Task<Purchase> CreateAsync(Purchase entity, object? session = null);
    Task<List<Purchase>> GetByRawMaterialIdAsync(string rawMaterialId);
    Task EnsureIndexesAsync();
}
