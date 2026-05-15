namespace Kelas.Domain.Models.Requests;

public class CreateExpenseRequest
{
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CashAccountId { get; set; } = string.Empty;
}