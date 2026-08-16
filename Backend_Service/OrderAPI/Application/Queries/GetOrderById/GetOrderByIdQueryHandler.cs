using MediatR;
using OrderAPI.Application.Common.Interfaces;
using OrderAPI.Application.Common.Models;
using OrderAPI.Data;

namespace OrderAPI.Application.Queries.GetOrderById;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, ApplicationResult<OrderDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetOrderByIdQueryHandler(
        ApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ApplicationResult<OrderDto>> Handle(
        GetOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        var order = await _context.Orders.FindAsync([request.Id], cancellationToken);
        if (order is null)
        {
            return ApplicationResult<OrderDto>.NotFound();
        }

        if (!_currentUserService.IsAdmin && order.OwnerId != _currentUserService.UserId)
        {
            return ApplicationResult<OrderDto>.Forbidden();
        }

        return ApplicationResult<OrderDto>.Success(OrderDto.FromEntity(order));
    }
}
