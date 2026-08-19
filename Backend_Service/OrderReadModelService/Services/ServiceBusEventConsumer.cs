using Azure.Messaging.ServiceBus;
using OrderReadModelService.Models;
using OrderReadModelService.Services;
using System.Text.Json;

namespace OrderReadModelService.Services;

/// <summary>
/// Background service that consumes domain events from Azure Service Bus queues
/// and projects them into the read model (Cosmos DB).
/// </summary>
public class ServiceBusEventConsumer : BackgroundService
{
    private readonly ServiceBusClient _client;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ServiceBusEventConsumer> _logger;
    private readonly List<ServiceBusProcessor> _processors = new();

    public ServiceBusEventConsumer(IConfiguration config, IServiceProvider serviceProvider, ILogger<ServiceBusEventConsumer> logger)
    {
        var connectionString = config["AzureServiceBus:ConnectionString"]
            ?? throw new InvalidOperationException("AzureServiceBus:ConnectionString not configured");
        _client = new ServiceBusClient(connectionString);
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            // Create processors for each queue
            var processors = new[]
            {
                _client.CreateProcessor("order-created"),
                _client.CreateProcessor("order-updated"),
                _client.CreateProcessor("order-deleted")
            };

            foreach (var processor in processors)
            {
                processor.ProcessMessageAsync += ProcessEventAsync;
                processor.ProcessErrorAsync += ErrorHandler;
                _processors.Add(processor);
            }

            // Start all processors
            foreach (var processor in _processors)
            {
                await processor.StartProcessingAsync(stoppingToken);
            }

            _logger.LogInformation("Service Bus event consumers started");

            // Keep the service running
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Service Bus event consumer cancellation requested");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in Service Bus event consumer");
            throw;
        }
    }

    private async Task ProcessEventAsync(ProcessMessageEventArgs args)
    {
        var body = args.Message.Body.ToString();
        var eventType = args.Message.Subject;

        _logger.LogInformation("Processing event of type {EventType}", eventType);

        try
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var repository = scope.ServiceProvider.GetRequiredService<IOrderReadModelRepository>();

                switch (eventType)
                {
                    case nameof(OrderCreatedEvent):
                        await HandleOrderCreatedAsync(body, repository, args.CancellationToken);
                        break;

                    case nameof(OrderUpdatedEvent):
                        await HandleOrderUpdatedAsync(body, repository, args.CancellationToken);
                        break;

                    case nameof(OrderDeletedEvent):
                        await HandleOrderDeletedAsync(body, repository, args.CancellationToken);
                        break;

                    default:
                        _logger.LogWarning("Unknown event type: {EventType}", eventType);
                        break;
                }

                await args.CompleteMessageAsync(args.Message);
                _logger.LogInformation("Event {EventType} processed successfully", eventType);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing event of type {EventType}. Message will be abandoned", eventType);
            await args.AbandonMessageAsync(args.Message);
        }
    }

    private async Task HandleOrderCreatedAsync(string eventJson, IOrderReadModelRepository repository, CancellationToken cancellationToken)
    {
        var @event = JsonSerializer.Deserialize<OrderCreatedEvent>(eventJson)
            ?? throw new InvalidOperationException("Failed to deserialize OrderCreatedEvent");

        var readModel = new OrderReadModel
        {
            OrderId = @event.OrderId,
            CustomerName = @event.CustomerName,
            TotalAmount = @event.TotalAmount,
            OwnerId = @event.OwnerId,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = DateTime.UtcNow
        };

        await repository.AddOrderAsync(readModel, cancellationToken);
    }

    private async Task HandleOrderUpdatedAsync(string eventJson, IOrderReadModelRepository repository, CancellationToken cancellationToken)
    {
        var @event = JsonSerializer.Deserialize<OrderUpdatedEvent>(eventJson)
            ?? throw new InvalidOperationException("Failed to deserialize OrderUpdatedEvent");

        await repository.UpdateOrderAsync(@event.OrderId, @event.CustomerName, @event.Status, @event.TotalAmount, cancellationToken);
    }

    private async Task HandleOrderDeletedAsync(string eventJson, IOrderReadModelRepository repository, CancellationToken cancellationToken)
    {
        var @event = JsonSerializer.Deserialize<OrderDeletedEvent>(eventJson)
            ?? throw new InvalidOperationException("Failed to deserialize OrderDeletedEvent");

        await repository.DeleteOrderAsync(@event.OrderId, cancellationToken);
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Service Bus processing error on {EntityPath}", args.EntityPath);
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Service Bus event consumers");

        foreach (var processor in _processors)
        {
            await processor.StopProcessingAsync(cancellationToken);
        }

        await base.StopAsync(cancellationToken);
    }
}
