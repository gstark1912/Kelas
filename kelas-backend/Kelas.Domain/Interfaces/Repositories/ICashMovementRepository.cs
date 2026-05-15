using Kelas.Domain.Entities;
using Kelas.Domain.Models.Requests;

namespace Kelas.Domain.Interfaces.Repositories;

public interface ICashMovementRepository
{
    Task<CashMovement> CreateAsync(CashMovement entity, object? session = null);
    Task<List<CashMovement>> GetByFiltersAsync(CashMovementFilterRequest filter);
    Task EnsureIndexesAsync();
}
