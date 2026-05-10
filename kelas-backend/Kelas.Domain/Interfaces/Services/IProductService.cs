using Kelas.Domain.Models.Requests;
using Kelas.Domain.Models.Responses;

namespace Kelas.Domain.Interfaces.Services;

public interface IProductService
{
    Task<List<ProductListResponse>> GetAllAsync(string? search = null);
    Task<ProductDetailResponse> GetByIdAsync(string id);
    Task<ProductDetailResponse> CreateAsync(CreateProductRequest request);
    Task<ProductDetailResponse> UpdateAsync(string id, UpdateProductRequest request);
    Task<ProductDetailResponse> UpdateRecipeAsync(string id, UpdateRecipeRequest request);
    Task<ProductDetailResponse> UpdateVisibilityAsync(string id, UpdateVisibilityRequest request);
}
