using Microsoft.AspNetCore.Connections;
using RabbitMQ.Client;
using Rating.Application.Contracts;
using System.Text;
using System.Text.Json;

namespace Rating.Infrastructure.Messaging
{
    public class RatingPublisher : IEventBus, IDisposable
    {
        private readonly IConnection _connection;
        private readonly RabbitMQ.Client.IModel _channel;

        public RatingPublisher(IConfiguration config)
        {
            var factory = new ConnectionFactory
            {
                HostName = config["RabbitMQ:HostName"],
                Port = int.Parse(config["RabbitMQ:Port"]),
                UserName = config["RabbitMQ:UserName"],
                Password = config["RabbitMQ:Password"]
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Khai báo exchange để publish (fanout để broadcast cho nhiều queue)
            _channel.ExchangeDeclare(
                exchange: "rating_exchange",
                type: ExchangeType.Fanout,
                durable: true
            );
        }

        public Task PublishAsync<T>(string exchangeName, T message)
        {
            _channel.ExchangeDeclare(exchangeName, ExchangeType.Fanout, durable: true);

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var props = _channel.CreateBasicProperties();
            props.Persistent = true;

            _channel.BasicPublish(exchange: exchangeName,
                                  routingKey: "",
                                  basicProperties: props,
                                  body: body);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}
