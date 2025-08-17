namespace ECommerce.Core.Entities
{
    public class Order
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserId { get; set; } = null!;
        public string ProductId { get; set; } = null!;
        public int Quantity { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
