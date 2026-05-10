namespace Kelas.Domain.Models.Responses;

public class ProductDetailResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal ListPrice { get; set; }
    public decimal? EstimatedHours { get; set; }
    public decimal? MinMargin { get; set; }
    public decimal EstimatedCost { get; set; }
    public decimal Margin { get; set; }
    public decimal CurrentStock { get; set; }
    public string MarginAlert { get; set; } = "ok"; // "ok" | "warning" | "danger"
    public List<RecipeItemResponse> Recipe { get; set; } = new();
    public bool IsVisible { get; set; }
    public string? Warning { get; set; } // Para ocultamiento con stock > 0
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
