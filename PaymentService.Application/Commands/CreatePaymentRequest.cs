namespace PaymentService.Application.Commands;

public sealed class CreatePaymentRequest
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "Card";
    public Guid? CorrelationId { get; set; }
}