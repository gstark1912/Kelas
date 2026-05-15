using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Kelas.Domain.Entities;

public class Expense
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("date")]
    public DateTime Date { get; set; }

    [BsonElement("amount")]
    public decimal Amount { get; set; }

    [BsonElement("category")]
    public string Category { get; set; } = string.Empty;

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("cashAccountId")]
    public ObjectId CashAccountId { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime? UpdatedAt { get; set; }
}