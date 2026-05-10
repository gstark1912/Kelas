namespace Kelas.Domain.Models.Responses;

public class StockAdjustmentResponse
{
    public string Id { get; set; } = string.Empty;
    public string RawMaterialId { get; set; } = string.Empty;
    public string RawMaterialName { get; set; } = string.Empty;
    public string MovementType { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string AdjustmentReason { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? Notes { get; set; }
    public decimal NewStock { get; set; }
    public string? Warning { get; set; }
    public DateTime CreatedAt { get; set; }
}
