using MediatR;

namespace PaymentService.Application.Commands
{
    public sealed record ProcessPaymentCommand(
       Guid OrderId,
       Guid UserId,
       decimal Amount,
       string PaymentMethod,
       Guid? CorrelationId
   ) : IRequest<Guid>;
}
