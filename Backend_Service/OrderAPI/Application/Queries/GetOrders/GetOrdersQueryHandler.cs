using MediatR;
using OrderAPI.Application.Common.Interfaces;
using OrderAPI.Application.Common.Models;

namespace OrderAPI.Application.Queries.GetOrders;

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, ApplicationResult<IReadOnlyList<OrderDto>>>
{
    private readonly IOrderReadModelRepository _readModelRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetOrdersQueryHandler(
        IOrderReadModelRepository readModelRepository,
        ICurrentUserService currentUserService)
    {
        _readModelRepository = readModelRepository;
        _currentUserService = currentUserService;
    }

    public async Task<ApplicationResult<IReadOnlyList<OrderDto>>> Handle(
        GetOrdersQuery request,
        CancellationToken cancellationToken)
    {
        //var orders = _currentUserService.IsAdmin
        //    ? await _readModelRepository.GetAllOrdersAsync(cancellationToken)
        //    : await _readModelRepository.GetOrdersByOwnerAsync(_currentUserService.UserId ?? string.Empty, cancellationToken);
        var orders = await _readModelRepository.GetAllOrdersAsync(cancellationToken);
        return ApplicationResult<IReadOnlyList<OrderDto>>.Success(orders);
    }
}
