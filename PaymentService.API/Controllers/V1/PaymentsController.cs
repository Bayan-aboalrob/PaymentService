using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlashSaleDB;
using PaymentService.Application.Commands;
using PaymentService.Application.Queries;

namespace PaymentService.API.Controllers.V1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly FlashSaleDbContext _ctx;

        public PaymentsController(IMediator mediator, FlashSaleDbContext ctx)
        {
            _mediator = mediator;
            _ctx = ctx;
        }

        // GET /api/v1/payments/{orderId}
        [HttpGet("{orderId:guid}")]
        public async Task<IActionResult> GetByOrder(Guid orderId, CancellationToken ct)
        {
            var payment = await _ctx.Payment
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.OrderId == orderId, ct);

            return payment is null ? NotFound() : Ok(payment);
        }

        public sealed class CreatePaymentRequest
        {
            public Guid OrderId { get; set; }
            public Guid UserId { get; set; }
            public decimal Amount { get; set; }
            public string PaymentMethod { get; set; } = "Card";
            public Guid? CorrelationId { get; set; }
        }

        public sealed record CreatePaymentResponse(
            Guid PaymentId,
            Guid OrderId,
            Guid UserId,
            string Status
        );

        // POST /api/v1/payments
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePaymentRequest req, CancellationToken ct)
        {
            var paymentId = await _mediator.Send(
                new ProcessPaymentCommand(
                    req.OrderId,
                    req.UserId,
                    req.Amount,
                    req.PaymentMethod,
                    req.CorrelationId
                ),
                ct);

            var payment = await _mediator.Send(new GetPaymentByIdQuery(paymentId), ct);

            if (payment is null)
            {
                return CreatedAtAction(nameof(GetByOrder), new { orderId = req.OrderId }, new { paymentId });
            }

            var response = new CreatePaymentResponse(
                PaymentId: payment.PaymentId,
                OrderId: payment.OrderId,
                UserId: payment.UserId,
                Status: payment.Status
            );

            return CreatedAtAction(
                nameof(GetByOrder),
                new { orderId = payment.OrderId },
                response
            );
        }
    }
}
