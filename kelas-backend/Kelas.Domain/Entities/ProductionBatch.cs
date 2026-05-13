using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Kelas.Domain.Entities;

public class ProductionBatch
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("productId")]
    public ObjectId ProductId { get; set; }

    [BsonElement("quantity")]
    public decimal Quantity { get; set; }

    [BsonElement("date")]
    public DateTime Date { get; set; }

    [BsonElement("totalCost")]
    public decimal TotalCost { get; set; }

    [BsonElement("unitCost")]
    public decimal UnitCost { get; set; }

    [BsonElement("ingredients")]
    public List<ProductionBatchIngredient> Ingredients { get; set; } = new();

    [BsonElement("notes")]
    public string? Notes { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime? UpdatedAt { get; set; }
}

public class ProductionBatchIngredient
{
    [BsonElement("rawMaterialId")]
    public ObjectId RawMaterialId { get; set; }

    [BsonElement("quantityUsed")]
    public decimal QuantityUsed { get; set; }

    [BsonElement("pricePerUnit")]
    public decimal PricePerUnit { get; set; }

    [BsonElement("cost")]
    public decimal Cost { get; set; }
}
