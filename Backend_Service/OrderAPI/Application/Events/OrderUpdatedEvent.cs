namespace OrderAPI.Application.Events;

/// <summary>
/// Published when an order is updated.
/// </summary>
public record OrderUpdatedEvent(
    int OrderId,
    string CustomerName,
    string Status,
    decimal? TotalAmount = null) : DomainEvent;
