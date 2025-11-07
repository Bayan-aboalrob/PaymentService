namespace PaymentService.Application.Contracts
{
    public interface IPaymentProcessor
    {
        Task<bool> ChargeAsync(Guid orderId, Guid userId, decimal amount, Guid? correlationId, CancellationToken ct);
    }
}
