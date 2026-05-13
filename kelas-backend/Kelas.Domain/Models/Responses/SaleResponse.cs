namespace Kelas.Domain.Models.Responses;

public class SaleResponse
{
    public string Id { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string CashAccountId { get; set; } = string.Empty;
    public List<SaleItemResponse> Items { get; set; } = new();
    public decimal SubtotalProductos { get; set; }
    public decimal ShippingCost { get; set; }
    public string? ShippingDetail { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxCostPercent { get; set; }
    public decimal TaxCostAmount { get; set; }
    public decimal ChannelCostPercent { get; set; }
    public decimal ChannelCostAmount { get; set; }
    public decimal GrossIncome { get; set; }
    public decimal TotalCOGS { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal NetProfit { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SaleItemResponse
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal UnitCost { get; set; }
    public decimal Subtotal { get; set; }
}
