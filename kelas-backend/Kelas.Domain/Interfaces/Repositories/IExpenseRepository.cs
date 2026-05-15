using Kelas.Domain.Entities;
using Kelas.Domain.Models.Requests;
using Kelas.Domain.Models.Responses;

namespace Kelas.Domain.Interfaces.Repositories;

public interface IExpenseRepository
{
    Task<Expense?> GetByIdAsync(string id);
    Task<List<Expense>> GetByFiltersAsync(ExpenseFilterRequest filter);
    Task<List<CategoryTotalResponse>> GetExpensesByCategoryAsync(DateTime from, DateTime to);
    Task<Expense> CreateAsync(Expense entity, object? session = null);
}