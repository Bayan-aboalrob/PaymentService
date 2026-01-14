namespace PaymentService.Application.Contracts
{
    public interface IBusPublisher
    {
        Task PublishAsync(string routingKey, object message, CancellationToken ct = default);
    }
}
