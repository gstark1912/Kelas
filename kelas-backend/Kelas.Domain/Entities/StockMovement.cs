using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Kelas.Domain.Entities;

public class StockMovement
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("itemType")]
    public string ItemType { get; set; } = string.Empty;

    [BsonElement("itemId")]
    public ObjectId ItemId { get; set; }

    [BsonElement("movementType")]
    public string MovementType { get; set; } = string.Empty;

    [BsonElement("quantity")]
    public decimal Quantity { get; set; }

    [BsonElement("date")]
    public DateTime Date { get; set; }

    [BsonElement("referenceType")]
    public string? ReferenceType { get; set; }

    [BsonElement("referenceId")]
    public ObjectId? ReferenceId { get; set; }

    [BsonElement("adjustmentReason")]
    public string? AdjustmentReason { get; set; }

    [BsonElement("notes")]
    public string? Notes { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }
}
