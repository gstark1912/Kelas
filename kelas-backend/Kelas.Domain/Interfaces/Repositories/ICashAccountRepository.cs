using Kelas.Domain.Entities;

namespace Kelas.Domain.Interfaces.Repositories;

public interface ICashAccountRepository
{
    Task<List<CashAccount>> GetActiveAsync();
    Task<CashAccount?> GetByIdAsync(string id);
    Task<CashAccount?> GetByNameAsync(string name);
    Task<CashAccount> CreateAsync(CashAccount entity);
    Task DecrementBalanceAsync(string id, decimal amount, object? session = null);
    Task EnsureIndexesAsync();
}
