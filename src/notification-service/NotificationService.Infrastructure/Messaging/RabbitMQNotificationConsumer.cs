using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NotificationService.Infrastructure.Settings;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Runtime;
using System.Text;
using System.Text.Json;

namespace NotificationService.Infrastructure.Messaging
{
    public abstract class RabbitMQBaseConsumer<T> : BackgroundService where T : class
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly RabbitMQSettings _settings;
        private readonly string _exchangeName;
        private readonly string _queueName;
        private readonly string _serviceName;

        private IConnection? _connection;
        private IModel? _channel;

        protected RabbitMQBaseConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMQSettings> options,
        string exchangeName,
        string queueName,
        string serviceName)
        {
            _scopeFactory = scopeFactory;
            _settings = options.Value;
            _exchangeName = exchangeName;
            _queueName = queueName;
            _serviceName = serviceName;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password
            };
            int retryCount = 0;
            while (_connection == null && retryCount < 10 && !stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _connection = factory.CreateConnection();
                }
                catch (Exception ex)
                {
                    retryCount++;
                    Console.WriteLine($"[WARN] RabbitMQ not ready (try {retryCount}/10): {ex.Message}");
                    Thread.Sleep(3000); // chờ 3s rồi thử lại
                }
            }

            if (_connection == null)
            {
                Console.WriteLine("[ERROR] Cannot connect to RabbitMQ after 10 retries.");
                return Task.CompletedTask;
            }
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(_exchangeName, ExchangeType.Fanout, durable: true);
            _channel.QueueDeclare(_queueName, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(_queueName, _exchangeName, "");

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (ch, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true // ✅ fix mismatch UserId/userId
                    };
                    var data = JsonSerializer.Deserialize<T>(json, options);


                    if (data != null)
                        await HandleMessageAsync(data, stoppingToken);

                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] RabbitMQ Consumer ({_serviceName}): {ex.Message}");
                }
            };

            _channel.BasicConsume(_queueName, autoAck: false, consumer);
            return Task.CompletedTask;
        }

        protected abstract Task HandleMessageAsync(T message, CancellationToken ct);

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}
