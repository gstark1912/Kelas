using Kelas.Domain.Models.Requests;
using Kelas.Domain.Models.Responses;
using MongoDB.Bson;

namespace Kelas.Domain.Interfaces.Services;

public interface ICashAccountService
{
    Task<List<CashAccountResponse>> GetAllAsync();
    Task<Dictionary<string, CashAccountResponse>> GetByIdsAsync(IEnumerable<ObjectId> ids);
    Task<CashAccountSummaryResponse> GetSummaryAsync();
    Task<CashAccountResponse> GetByIdAsync(string id);
    Task<CashAccountResponse> CreateAsync(CreateCashAccountRequest request);
    Task IncrementBalanceAsync(string accountId, decimal amount, object? session = null);
    Task DecrementBalanceAsync(string accountId, decimal amount, object? session = null);
    Task RegisterPaymentAsync(string accountId, decimal amount, string concept, string description, string referenceType, string referenceId, DateTime date, object? session = null);
}
