using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PaymentService.Application.Commands;
using PaymentService.Application.Dtos;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PaymentService.Infrastructure.Messaging
{
    internal sealed class OrderCreatedSingleConsumer : BackgroundService
    {
        private readonly ILogger<OrderCreatedSingleConsumer> _log;
        private readonly IServiceProvider _sp;
        private readonly IConnection _conn;
        private readonly IModel _ch;
        private readonly string _queue;

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public OrderCreatedSingleConsumer(
            ILogger<OrderCreatedSingleConsumer> log,
            IServiceProvider sp,
            IConfiguration cfg)
        {
            _log = log;
            _sp = sp;

            var section = cfg.GetSection("RabbitMQ");
            var factory = new ConnectionFactory
            {
                HostName = section["HostName"] ?? "localhost",
                Port = int.TryParse(section["Port"], out var p) ? p : 5672,
                VirtualHost = section["VirtualHost"] ?? "/",
                UserName = section["UserName"] ?? "guest",
                Password = section["Password"] ?? "guest",
                DispatchConsumersAsync = true
            };

            _conn = factory.CreateConnection("payment-consumer");
            _ch = _conn.CreateModel();

            var exchange = section["Exchange"] ?? "flashsale.topic";
            var exchangeType = section["ExchangeType"] ?? "topic";
            _ch.ExchangeDeclare(exchange, exchangeType, durable: true);

            _queue = "payment.order-created.v1";
            _ch.QueueDeclare(_queue, durable: true, exclusive: false, autoDelete: false);
            _ch.QueueBind(_queue, exchange, "Order.Created");

            _ch.BasicQos(0, 1, false);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_ch);
            consumer.Received += OnMessageAsync;
            _ch.BasicConsume(_queue, autoAck: false, consumer);
            _log.LogInformation("PaymentService is listening to Order.Created ...");
            return Task.CompletedTask;
        }

        private async Task OnMessageAsync(object sender, BasicDeliverEventArgs ea)
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var msg = JsonSerializer.Deserialize<OrderCreatedIntegrationEvent>(json, JsonOpts);

                if (msg is null)
                {
                    _ch.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                if (msg.Id == Guid.Empty)
                {
                    _log.LogWarning("Received Order.Created with empty Id, skipping.");
                    _ch.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                Guid? correlationId = null;
                if (!string.IsNullOrWhiteSpace(msg.CorrelationId) &&
                    Guid.TryParse(msg.CorrelationId, out var parsed))
                {
                    correlationId = parsed;
                }

                var attempt = 0;
                var processed = false;

                while (!processed && attempt < 3)
                {
                    attempt++;
                    try
                    {
                        using var scope = _sp.CreateScope();
                        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                        await mediator.Send(new ProcessPaymentCommand(
                            msg.Id,
                            msg.UserId,
                            msg.Total,
                            "Card",
                            correlationId,
                            PaymentExecutionMode.FireAndForgetBus
                        ), CancellationToken.None);

                        processed = true;
                    }
                    catch (DbUpdateException dbEx) when (
                        dbEx.InnerException?.Message.Contains("FK_Payment_Order_OrderId") == true)
                    {
                        _log.LogWarning("Order {OrderId} not yet visible, retrying ({Attempt}/3)...", msg.Id, attempt);
                        await Task.Delay(300);
                    }
                }

                _ch.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error handling Order.Created");
                _ch.BasicNack(ea.DeliveryTag, false, requeue: false);
            }
        }

        public override void Dispose()
        {
            _ch?.Close();
            _conn?.Close();
            base.Dispose();
        }
    }
}
