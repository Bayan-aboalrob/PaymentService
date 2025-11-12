using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlashSaleDB;
using PaymentService.Application.Commands;
using PaymentService.Application.Queries;

namespace PaymentService.API.Controllers.V2
{
    [ApiController]
    [Route("api/v2/Payments")]
    public class PaymentsControllerV2 : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly FlashSaleDbContext _ctx;

        public PaymentsControllerV2(IMediator mediator, FlashSaleDbContext ctx)
        {
            _mediator = mediator;
            _ctx = ctx;
        }

        // GET /api/v1/payments/{orderId}
        [HttpGet("{orderId:guid}")]
        public async Task<IActionResult> GetByOrder(string orderId, CancellationToken ct)
        {
            var payment = await _ctx.Payment
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.OrderId == Guid.Parse(orderId), ct);

            return payment is null ? NotFound() : Ok(payment);
        }

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
                    req.CorrelationId,
                    PaymentExecutionMode.FireAndForgetBus
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
