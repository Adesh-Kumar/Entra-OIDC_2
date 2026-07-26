using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderAPI.Models;

namespace OrderAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private static readonly List<Order> _orders = new List<Order>
        {
            new Order { Id = 1, CustomerName = "John Doe", TotalAmount = 150.00m, Status = "Shipped" },
            new Order { Id = 2, CustomerName = "Jane Smith", TotalAmount = 300.50m, Status = "Pending" }
        };

        [HttpGet]
        public ActionResult<IEnumerable<Order>> GetOrders()
        {
            return Ok(_orders);
        }

        [HttpGet("{id}")]
        public ActionResult<Order> GetOrder(int id)
        {
            var order = _orders.FirstOrDefault(o => o.Id == id);
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }

        [HttpPost]
        public ActionResult<Order> CreateOrder(Order order)
        {
            order.Id = _orders.Any() ? _orders.Max(o => o.Id) + 1 : 1;
            _orders.Add(order);
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateOrder(int id, Order order)
        {
            if (id != order.Id)
            {
                return BadRequest();
            }

            var existingOrder = _orders.FirstOrDefault(o => o.Id == id);
            if (existingOrder == null)
            {
                return NotFound();
            }

            existingOrder.CustomerName = order.CustomerName;
            existingOrder.TotalAmount = order.TotalAmount;
            existingOrder.Status = order.Status;

            return NoContent();
        }
    }
}
