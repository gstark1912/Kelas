namespace Kelas.Domain.Models.Requests;

public class CreateProductionBatchRequest
{
    public string ProductId { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public DateTime? Date { get; set; }
    public string? Notes { get; set; }
    public bool ConfirmInsufficientStock { get; set; }
}
