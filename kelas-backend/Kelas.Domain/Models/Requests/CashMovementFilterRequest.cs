namespace Kelas.Domain.Models.Requests;

public class CashMovementFilterRequest
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Type { get; set; }
    public string? Concept { get; set; }
    public string? CashAccountId { get; set; }
}
