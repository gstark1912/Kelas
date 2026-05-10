namespace Kelas.Domain.Models.Requests;

public class CreatePurchaseRequest
{
    public string RawMaterialId { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime? Date { get; set; }
    public string? Supplier { get; set; }
    public string CashAccountId { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public bool SkipPriceUpdate { get; set; }
}
