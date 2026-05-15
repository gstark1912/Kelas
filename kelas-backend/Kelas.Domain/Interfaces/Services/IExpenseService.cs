using Kelas.Domain.Models.Requests;
using Kelas.Domain.Models.Responses;

namespace Kelas.Domain.Interfaces.Services;

public interface IExpenseService
{
    Task<ExpenseListResponse> GetByFiltersAsync(ExpenseFilterRequest filter);
    Task<ExpenseResponse> GetByIdAsync(string id);
    Task<ExpenseResponse> CreateAsync(CreateExpenseRequest request);
    Task<List<CategoryTotalResponse>> GetExpensesByCategoryAsync(DateTime from, DateTime to);
}