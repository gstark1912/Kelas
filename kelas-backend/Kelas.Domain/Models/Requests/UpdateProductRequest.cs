namespace Kelas.Domain.Models.Requests;

public class UpdateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal ListPrice { get; set; }
    public decimal? EstimatedHours { get; set; }
    public decimal? MinMargin { get; set; }
}
