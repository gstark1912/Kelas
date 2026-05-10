using Kelas.Domain.Models.Requests;
using Kelas.Domain.Models.Responses;

namespace Kelas.Domain.Interfaces.Services;

public interface IRawMaterialService
{
    Task<List<RawMaterialListResponse>> GetAllAsync(string? search = null, string? unit = null);
    Task<RawMaterialDetailResponse> GetByIdAsync(string id);
    Task<RawMaterialDetailResponse> CreateAsync(CreateRawMaterialRequest request);
    Task<RawMaterialDetailResponse> UpdateAsync(string id, UpdateRawMaterialRequest request);
    Task DeleteAsync(string id);
}
