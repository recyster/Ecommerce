using System.Text.Json;
using ECommerce.Core.DTOs;
using ECommerce.Core.Entities;
using ECommerce.Infrastructure.Caching;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<OrdersController> _logger;
        private readonly IRabbitMqPublisher _publisher;
        private readonly IRedisService _redis;

        public OrdersController(AppDbContext db, ILogger<OrdersController> logger, IRabbitMqPublisher publisher, IRedisService redis)
        {
            _db = db;
            _logger = logger;
            _publisher = publisher;
            _redis = redis;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.UserId) || string.IsNullOrWhiteSpace(dto.ProductId) || dto.Quantity <= 0)
                return BadRequest("Invalid request");

            var order = new Order
            {
                UserId = dto.UserId,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                PaymentMethod = dto.PaymentMethod
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            _publisher.Publish(order, "order-placed");
            _redis.Remove($"orders:{dto.UserId}");

            _logger.LogInformation("Siparþ oluþturuldu ve kuyruða eklendi: {OrderId}", order.Id);
            return Ok(order);
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetOrders(string userId)
        {
            var cacheKey = $"orders:{userId}";
            var cached = _redis.GetString(cacheKey);
            if (!string.IsNullOrEmpty(cached))
            {
                _logger.LogInformation("Sipariþ geçici bellekten getirildi {UserId}", userId);
                var ordersFromCache = JsonSerializer.Deserialize<List<Order>>(cached);
                return Ok(ordersFromCache);
            }

            var orders = await _db.Orders.Where(o => o.UserId == userId).ToListAsync();
            _redis.SetString(cacheKey, JsonSerializer.Serialize(orders), TimeSpan.FromMinutes(2));
            return Ok(orders);
        }
    }
}
