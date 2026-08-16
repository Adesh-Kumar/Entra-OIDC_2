using MediatR;
using OrderAPI.Application.Common.Models;

namespace OrderAPI.Application.Commands.CreateOrder;

public record CreateOrderCommand(
    string CustomerName,
    decimal TotalAmount,
    string Status) : IRequest<ApplicationResult<OrderDto>>;
