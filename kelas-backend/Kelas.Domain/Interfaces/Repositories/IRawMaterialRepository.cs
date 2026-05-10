using Kelas.Domain.Entities;
using MongoDB.Bson;

namespace Kelas.Domain.Interfaces.Repositories;

public interface IRawMaterialRepository
{
    Task<List<RawMaterial>> GetActiveAsync(string? search = null, string? unit = null);
    Task<RawMaterial?> GetByIdAsync(string id);
    Task<RawMaterial?> GetByNameAsync(string name);
    Task<List<RawMaterial>> GetByIdsAsync(IEnumerable<ObjectId> ids);
    Task<RawMaterial> CreateAsync(RawMaterial entity, object? session = null);
    Task UpdateAsync(string id, RawMaterial entity);
    Task EnsureIndexesAsync();
}
