namespace Kelas.Domain.Models.Requests;

public class CreateStockAdjustmentRequest
{
    public string ItemType { get; set; } = string.Empty;
    public string ItemId { get; set; } = string.Empty;
    public decimal NewStock { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime? Date { get; set; }
    public string? Notes { get; set; }
}
