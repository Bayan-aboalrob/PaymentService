namespace PaymentService.Application.Commands;

public sealed record CreatePaymentResponse(
    Guid PaymentId,
    Guid OrderId,
    Guid UserId,
    string Status
);