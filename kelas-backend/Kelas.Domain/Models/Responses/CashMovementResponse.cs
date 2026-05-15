namespace Kelas.Domain.Models.Responses;

public class CashMovementResponse
{
    public string Id { get; set; } = string.Empty;
    public string CashAccountId { get; set; } = string.Empty;
    public string CashAccountName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Concept { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Origin { get; set; } = string.Empty;
    public string? ReferenceType { get; set; }
    public string? ReferenceId { get; set; }
    public string? LinkedMovementId { get; set; }
    public DateTime CreatedAt { get; set; }
}
