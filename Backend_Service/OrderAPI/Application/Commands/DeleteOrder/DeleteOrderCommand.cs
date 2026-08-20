using MediatR;
using OrderAPI.Application.Common.Models;

namespace OrderAPI.Application.Commands.DeleteOrder;

public record DeleteOrderCommand(int Id) : IRequest<ApplicationResult>;
