using OrderAPI.Application.Common.Models;

namespace OrderAPI.Application.Common.Interfaces;

public interface IOrderReadModelRepository
{
    Task<IReadOnlyList<OrderDto>> GetAllOrdersAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderDto>> GetOrdersByOwnerAsync(string ownerId, CancellationToken cancellationToken = default);
    Task<OrderDto?> GetOrderByIdAsync(int orderId, CancellationToken cancellationToken = default);
}
