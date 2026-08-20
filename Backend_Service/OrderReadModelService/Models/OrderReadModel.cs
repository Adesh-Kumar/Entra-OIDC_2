using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OrderReadModelService.Models;

/// <summary>
/// Denormalized read model for orders optimized for query performance.
/// Stored in Cosmos DB (MongoDB API).
/// </summary>
[BsonIgnoreExtraElements]
public class OrderReadModel
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("orderId")]
    public int OrderId { get; set; }

    [BsonElement("customerName")]
    public string CustomerName { get; set; } = string.Empty;

    [BsonElement("totalAmount")]
    public decimal TotalAmount { get; set; }

    [BsonElement("status")]
    public string Status { get; set; } = "Pending";

    [BsonElement("ownerId")]
    public string OwnerId { get; set; } = string.Empty;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("lastModifiedAt")]
    public DateTime LastModifiedAt { get; set; }
}
