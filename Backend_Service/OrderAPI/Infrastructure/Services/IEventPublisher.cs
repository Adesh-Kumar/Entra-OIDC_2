using OrderAPI.Application.Events;

namespace OrderAPI.Infrastructure.Services;

/// <summary>
/// Interface for publishing domain events to message bus.
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : DomainEvent;
}
