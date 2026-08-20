using Microsoft.AspNetCore.Mvc;
using OrderReadModelService.Models;
using OrderReadModelService.Services;

namespace OrderReadModelService.Controllers;

/// <summary>
/// Read model API for optimized order queries from Cosmos DB.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OrderReadModelsController : ControllerBase
{
    private readonly IOrderReadModelRepository _repository;
    private readonly ILogger<OrderReadModelsController> _logger;

    public OrderReadModelsController(IOrderReadModelRepository repository, ILogger<OrderReadModelsController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Get a specific order by ID from read model.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderReadModel>> GetOrder(int id, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _repository.GetOrderByIdAsync(id, cancellationToken);
            if (order is null)
            {
                _logger.LogWarning("Order {OrderId} not found in read model", id);
                return NotFound();
            }

            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order {OrderId} from read model", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving order");
        }
    }

    /// <summary>
    /// Get all orders for a specific user from read model.
    /// </summary>
    [HttpGet("user/{ownerId}")]
    public async Task<ActionResult<List<OrderReadModel>>> GetUserOrders(string ownerId, CancellationToken cancellationToken)
    {
        try
        {
            var orders = await _repository.GetAllOrdersAsync(ownerId, cancellationToken);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders for user {OwnerId} from read model", ownerId);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving orders");
        }
    }

    /// <summary>
    /// Health check endpoint for read model service.
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "Order Read Model Service is running" });
    }
}
