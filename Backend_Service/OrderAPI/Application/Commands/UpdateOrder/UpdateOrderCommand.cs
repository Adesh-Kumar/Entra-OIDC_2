using MediatR;
using OrderAPI.Application.Common.Models;

namespace OrderAPI.Application.Commands.UpdateOrder;

public record UpdateOrderCommand(
    int Id,
    string CustomerName,
    decimal TotalAmount,
    string Status) : IRequest<ApplicationResult>;
