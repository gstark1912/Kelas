using Kelas.Domain.Models.Requests;
using Kelas.Domain.Models.Responses;

namespace Kelas.Domain.Interfaces.Services;

public interface ICashAccountService
{
    Task<List<CashAccountResponse>> GetAllAsync();
    Task<CashAccountResponse> GetByIdAsync(string id);
    Task<CashAccountResponse> CreateAsync(CreateCashAccountRequest request);
}
