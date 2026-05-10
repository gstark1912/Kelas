namespace Kelas.Domain.Models.Responses;

public class ProductListResponse
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
}
