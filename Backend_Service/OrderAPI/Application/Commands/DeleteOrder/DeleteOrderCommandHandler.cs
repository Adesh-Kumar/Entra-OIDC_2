using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderAPI.Application.Common.Interfaces;
using OrderAPI.Application.Common.Models;
using OrderAPI.Application.Events;
using OrderAPI.Data;
using OrderAPI.Infrastructure.Services;

namespace OrderAPI.Application.Commands.DeleteOrder;

public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, ApplicationResult>
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<DeleteOrderCommandHandler> _logger;

    public DeleteOrderCommandHandler(
        ApplicationDbContext context,
        ICurrentUserService currentUserService,
        IEventPublisher eventPublisher,
        ILogger<DeleteOrderCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<ApplicationResult> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var existingOrder = await _context.Orders.FindAsync(request.Id, cancellationToken);
        if (existingOrder is null)
        {
            return ApplicationResult.NotFound();
        }

        if (!_currentUserService.IsAdmin && existingOrder.OwnerId != _currentUserService.UserId)
        {
            return ApplicationResult.Forbidden();
        }

        try
        {
            _context.Orders.Remove(existingOrder);
            await _context.SaveChangesAsync(cancellationToken);

            // Publish OrderDeletedEvent to Service Bus
            var @event = new OrderDeletedEvent(request.Id)
            {
                AggregateId = request.Id.ToString()
            };
            await _eventPublisher.PublishAsync(@event, cancellationToken);

            return ApplicationResult.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Orders.AnyAsync(o => o.Id == request.Id, cancellationToken))
            {
                return ApplicationResult.NotFound();
            }

            throw;
        }
    }
}
