namespace Kelas.Domain.Models.Requests;

public class ExpenseFilterRequest
{
    public string? Category { get; set; }
    public string? CashAccountId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}