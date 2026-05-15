namespace Kelas.Domain.Models.Responses;

public class ExpenseResponse
{
    public string Id { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CashAccountId { get; set; } = string.Empty;
    public string CashAccountName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}