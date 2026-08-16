using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderAPI.Application.Common.Interfaces;
using OrderAPI.Application.Common.Models;
using OrderAPI.Data;

namespace OrderAPI.Application.Queries.GetOrders;

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, ApplicationResult<IReadOnlyList<OrderDto>>>
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetOrdersQueryHandler(
        ApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ApplicationResult<IReadOnlyList<OrderDto>>> Handle(
        GetOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Orders.AsQueryable();

        if (!_currentUserService.IsAdmin)
        {
            query = query.Where(o => o.OwnerId == _currentUserService.UserId);
        }

        var orders = await query.ToListAsync(cancellationToken);

        return ApplicationResult<IReadOnlyList<OrderDto>>.Success(
            orders.Select(OrderDto.FromEntity).ToList());
    }
}
