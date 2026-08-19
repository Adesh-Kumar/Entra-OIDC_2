using MediatR;
using OrderAPI.Application.Common.Interfaces;
using OrderAPI.Application.Common.Models;
using OrderAPI.Application.Events;
using OrderAPI.Data;
using OrderAPI.Infrastructure.Services;
using OrderAPI.Models;

namespace OrderAPI.Application.Commands.CreateOrder;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, ApplicationResult<OrderDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<CreateOrderCommandHandler> _logger;

    public CreateOrderCommandHandler(
        ApplicationDbContext context,
        ICurrentUserService currentUserService,
        IEventPublisher eventPublisher,
        ILogger<CreateOrderCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<ApplicationResult<OrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new Order
        {
            CustomerName = string.IsNullOrWhiteSpace(request.CustomerName)
                ? _currentUserService.UserName ?? "Entra User"
                : request.CustomerName,
            TotalAmount = request.TotalAmount,
            Status = request.Status,
            OwnerId = _currentUserService.UserId ?? string.Empty
        };

        try
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync(cancellationToken);

            // Publish OrderCreatedEvent to Service Bus
            var @event = new OrderCreatedEvent(
                order.Id,
                order.CustomerName,
                order.TotalAmount,
                order.OwnerId)
            {
                AggregateId = order.Id.ToString()
            };
            await _eventPublisher.PublishAsync(@event, cancellationToken);

            return ApplicationResult<OrderDto>.Success(OrderDto.FromEntity(order));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create order for user {UserId}", _currentUserService.UserId);
            return ApplicationResult<OrderDto>.Failure(ex.GetBaseException().Message);
        }
    }
}
