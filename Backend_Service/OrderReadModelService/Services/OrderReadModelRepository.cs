using MongoDB.Driver;
using OrderReadModelService.Models;

namespace OrderReadModelService.Services;

/// <summary>
/// MongoDB/Cosmos DB implementation of order read model repository.
/// Projects domain events into denormalized read model.
/// </summary>
public class OrderReadModelRepository : IOrderReadModelRepository
{
    private readonly IMongoCollection<OrderReadModel> _collection;
    private readonly ILogger<OrderReadModelRepository> _logger;

    public OrderReadModelRepository(IMongoClient mongoClient, IConfiguration config, ILogger<OrderReadModelRepository> logger)
    {
        var databaseName = config["Cosmos:DatabaseName"] ?? "OrderReadModel";
        var collectionName = config["Cosmos:OrdersCollectionName"] ?? "orders";

        var database = mongoClient.GetDatabase(databaseName);
        _collection = database.GetCollection<OrderReadModel>(collectionName);
        _logger = logger;
    }

    public async Task AddOrderAsync(OrderReadModel order, CancellationToken cancellationToken = default)
    {
        try
        {
            await _collection.InsertOneAsync(order, cancellationToken: cancellationToken);
            _logger.LogInformation("Order {OrderId} inserted into read model", order.OrderId);
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            _logger.LogWarning("Order {OrderId} already exists in read model", order.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inserting order {OrderId} into read model", order.OrderId);
            throw;
        }
    }

    public async Task<OrderReadModel?> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(o => o.OrderId == orderId).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<OrderReadModel>> GetAllOrdersAsync(string ownerId, CancellationToken cancellationToken = default)
    {
        return await _collection
            .Find(o => o.OwnerId == ownerId)
            .SortByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateOrderAsync(int orderId, string customerName, string status, decimal? totalAmount = null, CancellationToken cancellationToken = default)
    {
        var update = Builders<OrderReadModel>.Update
            .Set(o => o.CustomerName, customerName)
            .Set(o => o.Status, status)
            .Set(o => o.LastModifiedAt, DateTime.UtcNow);

        if (totalAmount.HasValue)
        {
            update = update.Set(o => o.TotalAmount, totalAmount.Value);
        }

        try
        {
            var result = await _collection.UpdateOneAsync(
                o => o.OrderId == orderId,
                update,
                cancellationToken: cancellationToken);

            if (result.MatchedCount == 0)
            {
                _logger.LogWarning("Order {OrderId} not found in read model for update", orderId);
            }
            else
            {
                _logger.LogInformation("Order {OrderId} updated in read model", orderId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order {OrderId} in read model", orderId);
            throw;
        }
    }

    public async Task DeleteOrderAsync(int orderId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _collection.DeleteOneAsync(
                o => o.OrderId == orderId,
                cancellationToken: cancellationToken);

            if (result.DeletedCount == 0)
            {
                _logger.LogWarning("Order {OrderId} not found in read model for deletion", orderId);
            }
            else
            {
                _logger.LogInformation("Order {OrderId} deleted from read model", orderId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting order {OrderId} from read model", orderId);
            throw;
        }
    }
}
