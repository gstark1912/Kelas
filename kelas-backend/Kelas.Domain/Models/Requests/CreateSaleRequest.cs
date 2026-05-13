namespace Kelas.Domain.Models.Requests;

public class CreateSaleRequest
{
    public DateTime Date { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string CashAccountId { get; set; } = string.Empty;
    public List<CreateSaleItemRequest> Items { get; set; } = new();
    public decimal ShippingCost { get; set; }
    public string? ShippingDetail { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal TaxCostPercent { get; set; }
    public decimal ChannelCostPercent { get; set; }
    public string? Notes { get; set; }
}

public class CreateSaleItemRequest
{
    public string ProductId { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
}
