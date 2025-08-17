using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace ECommerce.Infrastructure.Messaging
{
    public interface IRabbitMqPublisher
    {
        void Publish<T>(T message, string queueName);
    }

    public class RabbitMqPublisher : IRabbitMqPublisher
    {
        private readonly ConnectionFactory _factory;

        public RabbitMqPublisher(string hostName)
        {
            _factory = new ConnectionFactory() { HostName = hostName };
        }

        public void Publish<T>(T message, string queueName)
        {
            using var connection = _factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);
            var props = channel.CreateBasicProperties();
            props.Persistent = true;
            channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: props, body: body);
        }
    }
}
