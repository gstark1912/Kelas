namespace Kelas.Domain.Models.Responses;

public class RecipeItemResponse
{
    public string RawMaterialId { get; set; } = string.Empty;
    public string RawMaterialName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal PricePerUnit { get; set; }
    public decimal Subtotal { get; set; } // Quantity × PricePerUnit
}
