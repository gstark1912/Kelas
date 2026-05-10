namespace Kelas.Domain.Models.Responses;

public class RawMaterialListResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal MinStock { get; set; }
    public decimal CurrentQuantity { get; set; }
    public decimal LastPricePerUnit { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? LastPurchaseDate { get; set; }
}
