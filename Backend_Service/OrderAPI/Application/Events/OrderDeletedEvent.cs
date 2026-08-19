namespace OrderAPI.Application.Events;

/// <summary>
/// Published when an order is deleted.
/// </summary>
public record OrderDeletedEvent(int OrderId) : DomainEvent;
