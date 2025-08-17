using ECommerce.Core.Entities;
using ECommerce.Infrastructure.Caching;
using ECommerce.Infrastructure.Messaging;

namespace ECommerce.Worker
{
    public class OrderProcessingWorker : BackgroundService
    {
        private readonly IRabbitMqConsumer _consumer;
        private readonly IRedisService _redis;
        private readonly ILogger<OrderProcessingWorker> _logger;

        public OrderProcessingWorker(IRabbitMqConsumer consumer, IRedisService redis, ILogger<OrderProcessingWorker> logger)
        {
            _consumer = consumer;
            _redis = redis;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Consume("order-placed", async (Order order) =>
            {
                _logger.LogInformation("��lenen sipari�: {OrderId}", order.Id);
                await Task.Delay(2000, stoppingToken); // simulate work
                var logMessage = $"��lenen tarih-saat {DateTime.UtcNow:o}";
                _redis.SetString($"order-log:{order.Id}", logMessage);
                _logger.LogInformation("Sipari� i�in bilgilendirme {OrderId}", order.Id);
            });
            return Task.CompletedTask;
        }
    }
}
