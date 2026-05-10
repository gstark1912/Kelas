namespace Kelas.Domain.Models.Responses;

public class PurchaseResponse
{
    public string Id { get; set; } = string.Empty;
    public string RawMaterialId { get; set; } = string.Empty;
    public string RawMaterialName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal PricePerUnit { get; set; }
    public DateTime Date { get; set; }
    public string? Supplier { get; set; }
    public string CashAccountId { get; set; } = string.Empty;
    public string CashAccountName { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
