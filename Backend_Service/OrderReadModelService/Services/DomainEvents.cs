using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace OrderReadModelService.Services;

/// <summary>
/// Domain events consumed from Service Bus.
/// These are shared event contracts with OrderAPI.
/// </summary>

public abstract record DomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public string AggregateId { get; init; } = string.Empty;
}

public record OrderCreatedEvent(
    int OrderId,
    string CustomerName,
    decimal TotalAmount,
    string OwnerId) : DomainEvent;

public record OrderUpdatedEvent(
    int OrderId,
    string CustomerName,
    string Status,
    decimal? TotalAmount = null) : DomainEvent;

public record OrderDeletedEvent(int OrderId) : DomainEvent;
