using Microsoft.Extensions.Logging;
using PaymentService.Application.Contracts;

namespace PaymentService.Infrastructure.Persistence.Services
{
    internal sealed class FakePaymentProcessor : IPaymentProcessor
    {
        private readonly ILogger<FakePaymentProcessor> _logger;

        public FakePaymentProcessor(ILogger<FakePaymentProcessor> logger)
        {
            _logger = logger;
        }

        public Task<bool> ChargeAsync(Guid orderId, Guid userId, decimal amount, Guid? correlationId, CancellationToken ct)
        {
            _logger.LogInformation("Charging user {UserId} for order {OrderId} amount {Amount} corr {CorrelationId}",
                userId, orderId, amount, correlationId);

            return Task.FromResult(true);
        }
    }
}
