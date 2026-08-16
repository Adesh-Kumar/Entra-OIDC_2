using OrderAPI.Models;

namespace OrderAPI.Application.Common.Models;

public class OrderDto
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;

    public static OrderDto FromEntity(Order order) => new()
    {
        Id = order.Id,
        CustomerName = order.CustomerName,
        TotalAmount = order.TotalAmount,
        Status = order.Status,
        OwnerId = order.OwnerId
    };
}
