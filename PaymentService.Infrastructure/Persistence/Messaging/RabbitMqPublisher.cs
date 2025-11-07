using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using PaymentService.Application.Contracts;
using RabbitMQ.Client;

namespace PaymentService.Infrastructure.Messaging
{
    internal sealed class RabbitMqPublisher : IBusPublisher, IDisposable
    {
        private readonly IConnection _conn;
        private readonly IModel _ch;
        private readonly string _exchange;

        public RabbitMqPublisher(IConfiguration cfg)
        {
            var section = cfg.GetSection("RabbitMQ");
            var factory = new ConnectionFactory
            {
                HostName = section["HostName"] ?? "localhost",
                Port = int.TryParse(section["Port"], out var p) ? p : 5672,
                VirtualHost = section["VirtualHost"] ?? "/",
                UserName = section["UserName"] ?? "guest",
                Password = section["Password"] ?? "guest"
            };

            _conn = factory.CreateConnection("payment-publisher");
            _ch = _conn.CreateModel();

            _exchange = section["Exchange"] ?? "flashsale.topic";
            var exchangeType = section["ExchangeType"] ?? "topic";
            _ch.ExchangeDeclare(_exchange, exchangeType, durable: true);
        }

        public Task PublishAsync(string routingKey, object message, CancellationToken ct = default)
        {
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            _ch.BasicPublish(_exchange, routingKey, basicProperties: null, body);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _ch?.Close();
            _conn?.Close();
        }
    }
}
