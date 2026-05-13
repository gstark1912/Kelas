using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Kelas.Domain.Entities;

public class Sale
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("date")]
    public DateTime Date { get; set; }

    [BsonElement("channel")]
    public string Channel { get; set; } = string.Empty;

    [BsonElement("paymentMethod")]
    public string PaymentMethod { get; set; } = string.Empty;

    [BsonElement("cashAccountId")]
    public ObjectId CashAccountId { get; set; }

    [BsonElement("items")]
    public List<SaleItem> Items { get; set; } = new();

    [BsonElement("subtotalProductos")]
    public decimal SubtotalProductos { get; set; }

    [BsonElement("shippingCost")]
    public decimal ShippingCost { get; set; }

    [BsonElement("shippingDetail")]
    public string? ShippingDetail { get; set; }

    [BsonElement("discountPercent")]
    public decimal DiscountPercent { get; set; }

    [BsonElement("discountAmount")]
    public decimal DiscountAmount { get; set; }

    [BsonElement("taxCostPercent")]
    public decimal TaxCostPercent { get; set; }

    [BsonElement("taxCostAmount")]
    public decimal TaxCostAmount { get; set; }

    [BsonElement("channelCostPercent")]
    public decimal ChannelCostPercent { get; set; }

    [BsonElement("channelCostAmount")]
    public decimal ChannelCostAmount { get; set; }

    [BsonElement("grossIncome")]
    public decimal GrossIncome { get; set; }

    [BsonElement("totalCOGS")]
    public decimal TotalCOGS { get; set; }

    [BsonElement("grossProfit")]
    public decimal GrossProfit { get; set; }

    [BsonElement("netProfit")]
    public decimal NetProfit { get; set; }

    [BsonElement("notes")]
    public string? Notes { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime? UpdatedAt { get; set; }
}

public class SaleItem
{
    [BsonElement("productId")]
    public ObjectId ProductId { get; set; }

    [BsonElement("productName")]
    public string ProductName { get; set; } = string.Empty;

    [BsonElement("quantity")]
    public decimal Quantity { get; set; }

    [BsonElement("unitPrice")]
    public decimal UnitPrice { get; set; }

    [BsonElement("unitCost")]
    public decimal UnitCost { get; set; }

    [BsonElement("subtotal")]
    public decimal Subtotal { get; set; }
}
