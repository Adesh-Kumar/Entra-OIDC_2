using MediatR;
using OrderAPI.Application.Common.Models;

namespace OrderAPI.Application.Queries.GetOrders;

public record GetOrdersQuery : IRequest<ApplicationResult<IReadOnlyList<OrderDto>>>;
