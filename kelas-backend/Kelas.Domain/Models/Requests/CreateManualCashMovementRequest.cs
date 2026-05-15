namespace Kelas.Domain.Models.Requests;

public class CreateManualCashMovementRequest
{
    public string CashAccountId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Concept { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? Description { get; set; }
}
