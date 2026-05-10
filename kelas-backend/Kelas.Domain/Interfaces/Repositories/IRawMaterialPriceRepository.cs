using Kelas.Domain.Entities;

namespace Kelas.Domain.Interfaces.Repositories;

public interface IRawMaterialPriceRepository
{
    Task<RawMaterialPrice> CreateAsync(RawMaterialPrice entity, object? session = null);
    Task EnsureIndexesAsync();
}
