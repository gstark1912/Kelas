namespace Kelas.Domain.Models.Responses;

public class ExpenseListResponse
{
    public List<ExpenseResponse> Items { get; set; } = new();
    public ExpenseKpiResponse Kpis { get; set; } = new();
}