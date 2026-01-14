using System;
using MediatR;

namespace PaymentService.Application.Queries
{
    public sealed record GetPaymentByIdQuery(Guid PaymentId) : IRequest<PaymentDetailsDto>;
    public sealed record PaymentDetailsDto(
        Guid PaymentId,
        Guid OrderId,
        Guid UserId,
        decimal Amount,
        string PaymentMethod,
        string Status,
        string? CorrelationId
    );
}
