using Kelas.Domain.Entities;
using MongoDB.Bson;

namespace Kelas.Domain.Interfaces.Repositories;

public interface ICashAccountRepository
{
    Task<List<CashAccount>> GetActiveAsync();
    Task<List<CashAccount>> GetByIdsAsync(IEnumerable<ObjectId> ids);
    Task<CashAccount?> GetByIdAsync(string id);
    Task<CashAccount?> GetByNameAsync(string name);
    Task<CashAccount> CreateAsync(CashAccount entity);
    Task DecrementBalanceAsync(string id, decimal amount, object? session = null);
    Task IncrementBalanceAsync(string id, decimal amount, object? session = null);
    Task EnsureIndexesAsync();
}
