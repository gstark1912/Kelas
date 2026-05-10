namespace Kelas.Domain.Models.Requests;

public class CreateStockAdjustmentRequest
{
    public string RawMaterialId { get; set; } = string.Empty;
    public decimal NewStock { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime? Date { get; set; }
    public string? Notes { get; set; }
}
