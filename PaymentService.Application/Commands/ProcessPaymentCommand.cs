using MediatR;

namespace PaymentService.Application.Commands
{
    public enum PaymentExecutionMode
    {
        Synchronous = 0,
        FireAndForgetBus = 1
    }
    public sealed record ProcessPaymentCommand(
       Guid OrderId,
       Guid UserId,
       decimal Amount,
       string PaymentMethod,
       Guid? CorrelationId,
       PaymentExecutionMode ExecutionMode
   ) : IRequest<Guid>;
}
