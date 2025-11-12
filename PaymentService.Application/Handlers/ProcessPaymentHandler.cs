using FlashSaleDB.Entities;
using MediatR;
using PaymentService.Application.Commands;
using PaymentService.Application.Contracts;
using PaymentService.Application.Services;

namespace PaymentService.Application.Payments.Handlers
{
    public sealed class ProcessPaymentHandler
        : IRequestHandler<ProcessPaymentCommand, Guid>
    {
        private readonly IPaymentRepository _repo;
        private readonly IPaymentProcessor _processor;
        private readonly IBusPublisher _bus;
        private readonly IHttpClientUtils _httpClient;

        public ProcessPaymentHandler(
            IPaymentRepository repo,
            IPaymentProcessor processor,
            IBusPublisher bus,
            IHttpClientUtils httpClient)
        {
            _repo = repo;
            _processor = processor;
            _bus = bus;
            _httpClient = httpClient;
        }

        public async Task<Guid> Handle(ProcessPaymentCommand request, CancellationToken ct)
        {
            var existing = await _repo.GetByOrderIdAsync(request.OrderId, ct);
            if (existing is not null)
                return existing.Id;

            var tries = 0;
            var exists = await _repo.OrderExistsAsync(request.OrderId, ct);
            while (!exists && tries < 3)
            {
                tries++;
                await Task.Delay(300, ct);
                exists = await _repo.OrderExistsAsync(request.OrderId, ct);
            }

            if (!exists)
            {
                throw new InvalidOperationException($"Order {request.OrderId} not found when processing payment.");
            }

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                OrderId = request.OrderId,
                UserId = request.UserId,
                Amount = request.Amount,
                PaymentMethod = request.PaymentMethod,
                CorrelationId = request.CorrelationId?.ToString(),
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(payment, ct);
            await _repo.SaveChangesAsync(ct);

            var ok = await _processor.ChargeAsync(
                request.OrderId,
                request.UserId,
                request.Amount,
                request.CorrelationId,
                ct
            );

            payment.Status = ok ? "Succeeded" : "Failed";
            payment.UpdatedAt = DateTime.UtcNow;
            await _repo.SaveChangesAsync(ct);

            if (request.ExecutionMode == PaymentExecutionMode.Synchronous)
            {
                // https://localhost/order
                await _httpClient.SendHttpRequest($"http://localhost/order/api/v1/Orders/{payment.OrderId}/status", new { NewStatus = ok ? "Paid" : "PendingPayment" }, HttpMethod.Put);
                
                // https://localhost/inventory
                await _httpClient.SendHttpRequest("http://localhost/inventory/api/v1/inventory/cache/apply-order", new { orderId = payment.OrderId }, HttpMethod.Post);
            }
            else
            {
                await _bus.PublishAsync(ok ? "Payment.Succeeded" : "Payment.Failed", new
                {
                    payment.Id,
                    payment.OrderId,
                    payment.UserId,
                    payment.Amount,
                    payment.PaymentMethod,
                    payment.Status,
                    payment.CorrelationId
                }, ct);   
            }

            return payment.Id;
        }
    }
}
