using Kelas.Domain.Models.Requests;
using Kelas.Domain.Models.Responses;

namespace Kelas.Domain.Interfaces.Services;

public interface ICashMovementService
{
    Task<CashMovementResponse> CreateManualAsync(CreateManualCashMovementRequest request);
    Task<CashTransferResponse> CreateTransferAsync(CreateCashTransferRequest request);
    Task<CashMovementListResponse> GetByFiltersAsync(CashMovementFilterRequest filter);
}
