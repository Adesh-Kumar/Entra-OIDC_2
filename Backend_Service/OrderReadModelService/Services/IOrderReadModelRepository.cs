using OrderReadModelService.Models;

namespace OrderReadModelService.Services;

/// <summary>
/// Repository interface for order read model operations.
/// </summary>
public interface IOrderReadModelRepository
{
    Task AddOrderAsync(OrderReadModel order, CancellationToken cancellationToken = default);
    Task<OrderReadModel?> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken = default);
    Task<List<OrderReadModel>> GetAllOrdersAsync(string ownerId, CancellationToken cancellationToken = default);
    Task UpdateOrderAsync(int orderId, string customerName, string status, decimal? totalAmount = null, CancellationToken cancellationToken = default);
    Task DeleteOrderAsync(int orderId, CancellationToken cancellationToken = default);
}
