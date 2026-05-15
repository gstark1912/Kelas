namespace Kelas.Domain.Models.Responses;

public class CashMovementListResponse
{
    public List<CashMovementResponse> Items { get; set; } = new();
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal NetTotal { get; set; }
}
