namespace OrderAPI.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending";
        public string OwnerId { get; set; } = string.Empty; // Entra ID Object ID (oid/sub claim)
    }
}
