using System.Text;
using System.Text.Json;
using ECommerce.Core.Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ECommerce.Infrastructure.Messaging
{
    public interface IRabbitMqConsumer
    {
        void Consume(string queueName, Action<Order> onMessage);
    }

    public class RabbitMqConsumer : IRabbitMqConsumer
    {
        private readonly ConnectionFactory _factory;

        public RabbitMqConsumer(string hostName)
        {
            _factory = new ConnectionFactory() { HostName = hostName };
        }

        public void Consume(string queueName, Action<Order> onMessage)
        {
            var connection = _factory.CreateConnection();
            var channel = connection.CreateModel();
            channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var order = JsonSerializer.Deserialize<Order>(message);
                if (order != null) onMessage(order);
                channel.BasicAck(ea.DeliveryTag, multiple: false);
            };
            channel.BasicQos(0, 1, false);
            channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
        }
    }
}
