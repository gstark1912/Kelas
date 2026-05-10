namespace Kelas.Domain.Models.Responses;

public class StockMovementResponse
{
    public string Id { get; set; } = string.Empty;
    public string MovementType { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public DateTime Date { get; set; }
    public string? ReferenceType { get; set; }
    public string? ReferenceId { get; set; }
    public string? AdjustmentReason { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
