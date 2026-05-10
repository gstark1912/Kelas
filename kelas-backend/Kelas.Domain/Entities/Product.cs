using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Kelas.Domain.Entities;

public class Product
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("listPrice")]
    public decimal ListPrice { get; set; }

    [BsonElement("estimatedHours")]
    public decimal? EstimatedHours { get; set; }

    [BsonElement("minMargin")]
    public decimal? MinMargin { get; set; }

    [BsonElement("recipe")]
    public List<RecipeItem> Recipe { get; set; } = new();

    [BsonElement("isVisible")]
    public bool IsVisible { get; set; } = true;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime? UpdatedAt { get; set; }
}

public class RecipeItem
{
    [BsonElement("rawMaterialId")]
    public ObjectId RawMaterialId { get; set; }

    [BsonElement("quantity")]
    public decimal Quantity { get; set; }
}
