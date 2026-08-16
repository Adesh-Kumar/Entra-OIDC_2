using MediatR;
using OrderAPI.Application.Common.Models;

namespace OrderAPI.Application.Queries.GetOrderById;

public record GetOrderByIdQuery(int Id) : IRequest<ApplicationResult<OrderDto>>;
