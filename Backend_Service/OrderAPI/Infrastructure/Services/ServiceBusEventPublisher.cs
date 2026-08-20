using Azure.Messaging.ServiceBus;
using OrderAPI.Application.Events;
using System.Text.Json;

namespace OrderAPI.Infrastructure.Services;

/// <summary>
/// Publishes domain events to Azure Service Bus queues.
/// Events are serialized as JSON and sent to corresponding queue based on event type.
/// </summary>
public class ServiceBusEventPublisher : IEventPublisher
{
    private readonly ServiceBusClient _client;
    private readonly ILogger<ServiceBusEventPublisher> _logger;

    public ServiceBusEventPublisher(IConfiguration config, ILogger<ServiceBusEventPublisher> logger)
    {
        var connectionString = config["AzureServiceBus:ConnectionString"]
            ?? throw new InvalidOperationException("AzureServiceBus:ConnectionString not configured");
        _client = new ServiceBusClient(connectionString);
        _logger = logger;
    }

    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : DomainEvent
    {
        var queueName = GetQueueNameForEvent(typeof(T).Name);

        try
        {
            var sender = _client.CreateSender(queueName);
            var eventJson = JsonSerializer.Serialize(@event);
            var message = new ServiceBusMessage(eventJson)
            {
                Subject = typeof(T).Name,
                MessageId = @event.EventId.ToString(),
                ApplicationProperties =
                {
                    ["EventType"] = typeof(T).Name,
                    ["OccurredAt"] = @event.OccurredAt
                }
            };

            await sender.SendMessageAsync(message, cancellationToken);
            _logger.LogInformation("Event {EventType} published to queue {QueueName}. EventId: {EventId}", 
                typeof(T).Name, queueName, @event.EventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event {EventType} to queue {QueueName}", typeof(T).Name, queueName);
            throw;
        }
    }

    private string GetQueueNameForEvent(string eventTypeName)
    {
        // Map event types to queue names
        return eventTypeName switch
        {
            nameof(OrderCreatedEvent) => "order-created",
            nameof(OrderUpdatedEvent) => "order-updated",
            nameof(OrderDeletedEvent) => "order-deleted",
            _ => "order-events"
        };
    }
}
