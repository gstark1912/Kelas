using Kelas.Domain.Entities;

namespace Kelas.Domain.Interfaces.Repositories;

public interface IProductionBatchRepository
{
    Task<ProductionBatch> CreateAsync(ProductionBatch entity, object? session = null);
    Task<ProductionBatch?> GetByIdAsync(string id);
    Task<List<ProductionBatch>> GetAsync(string? productId = null, DateTime? dateFrom = null, DateTime? dateTo = null);
    Task EnsureIndexesAsync();
}
