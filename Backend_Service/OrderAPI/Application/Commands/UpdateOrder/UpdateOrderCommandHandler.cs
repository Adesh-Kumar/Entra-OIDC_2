using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderAPI.Application.Common.Interfaces;
using OrderAPI.Application.Common.Models;
using OrderAPI.Data;

namespace OrderAPI.Application.Commands.UpdateOrder;

public class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, ApplicationResult>
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateOrderCommandHandler(
        ApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ApplicationResult> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var existingOrder = await _context.Orders.FindAsync([request.Id], cancellationToken);
        if (existingOrder is null)
        {
            return ApplicationResult.NotFound();
        }

        if (!_currentUserService.IsAdmin && existingOrder.OwnerId != _currentUserService.UserId)
        {
            return ApplicationResult.Forbidden();
        }

        existingOrder.CustomerName = request.CustomerName;
        existingOrder.TotalAmount = request.TotalAmount;
        existingOrder.Status = request.Status;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
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
