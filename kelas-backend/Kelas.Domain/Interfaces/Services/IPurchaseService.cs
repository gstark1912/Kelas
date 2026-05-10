using Kelas.Domain.Models.Requests;
using Kelas.Domain.Models.Responses;

namespace Kelas.Domain.Interfaces.Services;

public interface IPurchaseService
{
    Task<PurchaseResponse> CreateAsync(CreatePurchaseRequest request);
    Task<List<PurchaseResponse>> GetByRawMaterialAsync(string rawMaterialId);
}
