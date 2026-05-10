using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Kelas.Domain.Entities;

public class CashMovement
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("cashAccountId")]
    public ObjectId CashAccountId { get; set; }

    [BsonElement("type")]
    public string Type { get; set; } = string.Empty;

    [BsonElement("concept")]
    public string Concept { get; set; } = string.Empty;

    [BsonElement("amount")]
    public decimal Amount { get; set; }

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("date")]
    public DateTime Date { get; set; }

    [BsonElement("origin")]
    public string Origin { get; set; } = string.Empty;

    [BsonElement("referenceType")]
    public string? ReferenceType { get; set; }

    [BsonElement("referenceId")]
    public ObjectId? ReferenceId { get; set; }

    [BsonElement("linkedMovementId")]
    public ObjectId? LinkedMovementId { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }
}
