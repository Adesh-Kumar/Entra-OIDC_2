namespace OrderAPI.Application.Events;

/// <summary>
/// Published when a new order is created.
/// </summary>
public record OrderCreatedEvent(
    int OrderId,
    string CustomerName,
    decimal TotalAmount,
    string OwnerId) : DomainEvent;
