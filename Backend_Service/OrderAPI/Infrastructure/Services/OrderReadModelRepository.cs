using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using OrderAPI.Application.Common.Interfaces;
using OrderAPI.Application.Common.Models;

namespace OrderAPI.Infrastructure.Services;

public class OrderReadModelRepository : IOrderReadModelRepository
{
    private readonly IMongoCollection<OrderReadModelDocument> _collection;

    public OrderReadModelRepository(IMongoClient mongoClient, IConfiguration configuration)
    {
        var databaseName = configuration["Cosmos:DatabaseName"] ?? "OrderReadModel";
        var collectionName = configuration["Cosmos:OrdersCollectionName"] ?? "orders";

        var database = mongoClient.GetDatabase(databaseName);
        _collection = database.GetCollection<OrderReadModelDocument>(collectionName);
    }

    public async Task<IReadOnlyList<OrderDto>> GetAllOrdersAsync(CancellationToken cancellationToken = default)
    {
        var documents = await _collection
            .Find(Builders<OrderReadModelDocument>.Filter.Empty)
            .SortByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        return documents.Select(ToDto).ToList();
    }

    public async Task<IReadOnlyList<OrderDto>> GetOrdersByOwnerAsync(string ownerId, CancellationToken cancellationToken = default)
    {
        var documents = await _collection
            .Find(o => o.OwnerId == ownerId)
            .SortByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        return documents.Select(ToDto).ToList();
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var document = await _collection
            .Find(o => o.OrderId == orderId)
            .FirstOrDefaultAsync(cancellationToken);

        return document is null ? null : ToDto(document);
    }

    private static OrderDto ToDto(OrderReadModelDocument order) => new()
    {
        Id = order.OrderId,
        CustomerName = order.CustomerName,
        TotalAmount = order.TotalAmount,
        Status = order.Status,
        OwnerId = order.OwnerId
    };

    [BsonIgnoreExtraElements]
    private sealed class OrderReadModelDocument
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
    }
}
