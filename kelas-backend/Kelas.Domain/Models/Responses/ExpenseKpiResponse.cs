namespace Kelas.Domain.Models.Responses;

public class ExpenseKpiResponse
{
    public decimal TotalAmount { get; set; }
    public int TotalCount { get; set; }
    public CategoryTotalResponse? TopCategory { get; set; }
}