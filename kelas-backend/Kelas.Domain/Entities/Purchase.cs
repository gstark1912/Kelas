using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Kelas.Domain.Entities;

public class Purchase
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("rawMaterialId")]
    public ObjectId RawMaterialId { get; set; }

    [BsonElement("quantity")]
    public decimal Quantity { get; set; }

    [BsonElement("totalPrice")]
    public decimal TotalPrice { get; set; }

    [BsonElement("pricePerUnit")]
    public decimal PricePerUnit { get; set; }

    [BsonElement("date")]
    public DateTime Date { get; set; }

    [BsonElement("supplier")]
    public string? Supplier { get; set; }

    [BsonElement("cashAccountId")]
    public ObjectId CashAccountId { get; set; }

    [BsonElement("notes")]
    public string? Notes { get; set; }

    [BsonElement("skipPriceUpdate")]
    public bool SkipPriceUpdate { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }
}
