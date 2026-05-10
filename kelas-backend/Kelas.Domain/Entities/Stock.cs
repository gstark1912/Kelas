using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Kelas.Domain.Entities;

public class Stock
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("itemType")]
    public string ItemType { get; set; } = string.Empty;

    [BsonElement("itemId")]
    public ObjectId ItemId { get; set; }

    [BsonElement("currentQuantity")]
    public decimal CurrentQuantity { get; set; }

    [BsonElement("lastUpdated")]
    public DateTime? LastUpdated { get; set; }
}
