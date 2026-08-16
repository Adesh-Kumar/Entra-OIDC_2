using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderAPI.Application.Commands.CreateOrder;
using OrderAPI.Application.Commands.UpdateOrder;
using OrderAPI.Application.Common.Models;
using OrderAPI.Application.Queries.GetOrderById;
using OrderAPI.Application.Queries.GetOrders;

namespace OrderAPI.Controllers;

/// <summary>
/// Thin controller that dispatches CQRS commands and queries via MediatR.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetOrders(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetOrdersQuery(), cancellationToken);
        return ToActionResult(result, value => Ok(value));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrder(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetOrderByIdQuery(id), cancellationToken);
        return ToActionResult(result, value => Ok(value));
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder(
        [FromBody] CreateOrderCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return ToActionResult(
            result,
            value => CreatedAtAction(nameof(GetOrder), new { id = value.Id }, value),
            failureStatusCode: StatusCodes.Status500InternalServerError);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrder(
        int id,
        [FromBody] UpdateOrderCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }

        var result = await _mediator.Send(command, cancellationToken);
        return ToActionResult(result, () => NoContent());
    }

    private IActionResult ToActionResult(
        ApplicationResult result,
        Func<IActionResult> onSuccess,
        int failureStatusCode = StatusCodes.Status400BadRequest)
    {
        return result.Status switch
        {
            ApplicationResultStatus.Success => onSuccess(),
            ApplicationResultStatus.NotFound => NotFound(),
            ApplicationResultStatus.Forbidden => Forbid(),
            ApplicationResultStatus.Failure => Problem(
                title: "Request failed",
                detail: result.ErrorMessage,
                statusCode: failureStatusCode),
            _ => Problem(title: "Unexpected result", statusCode: StatusCodes.Status500InternalServerError)
        };
    }

    private ActionResult<T> ToActionResult<T>(
        ApplicationResult<T> result,
        Func<T, ActionResult<T>> onSuccess,
        int failureStatusCode = StatusCodes.Status400BadRequest)
    {
        return result.Status switch
        {
            ApplicationResultStatus.Success when result.Value is not null => onSuccess(result.Value),
            ApplicationResultStatus.NotFound => NotFound(),
            ApplicationResultStatus.Forbidden => Forbid(),
            ApplicationResultStatus.Failure => Problem(
                title: "Request failed",
                detail: result.ErrorMessage,
                statusCode: failureStatusCode),
            _ => Problem(title: "Unexpected result", statusCode: StatusCodes.Status500InternalServerError)
        };
    }
}
