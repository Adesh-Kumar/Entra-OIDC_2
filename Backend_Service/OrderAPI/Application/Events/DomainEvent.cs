namespace OrderAPI.Application.Events;

/// <summary>
/// Base class for all domain events.
/// </summary>
public abstract record DomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public string AggregateId { get; init; } = string.Empty;
}
