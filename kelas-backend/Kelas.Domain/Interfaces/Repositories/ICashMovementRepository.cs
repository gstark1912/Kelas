using Kelas.Domain.Entities;

namespace Kelas.Domain.Interfaces.Repositories;

public interface ICashMovementRepository
{
    Task<CashMovement> CreateAsync(CashMovement entity, object? session = null);
    Task EnsureIndexesAsync();
}
