using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Kelas.Domain.Entities;

public class RawMaterialPrice
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("rawMaterialId")]
    public ObjectId RawMaterialId { get; set; }

    [BsonElement("pricePerUnit")]
    public decimal PricePerUnit { get; set; }

    [BsonElement("dateFrom")]
    public DateTime DateFrom { get; set; }

    [BsonElement("purchaseId")]
    public ObjectId PurchaseId { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }
}
