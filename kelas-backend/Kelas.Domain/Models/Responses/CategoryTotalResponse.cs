namespace Kelas.Domain.Models.Responses;

public class CategoryTotalResponse
{
    public string Category { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public int Count { get; set; }
}