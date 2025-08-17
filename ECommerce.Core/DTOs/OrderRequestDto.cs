namespace ECommerce.Core.DTOs
{
    public class OrderRequestDto
    {
        public string UserId { get; set; } = null!;
        public string ProductId { get; set; } = null!;
        public int Quantity { get; set; }
        public string PaymentMethod { get; set; } = null!;
    }
}
