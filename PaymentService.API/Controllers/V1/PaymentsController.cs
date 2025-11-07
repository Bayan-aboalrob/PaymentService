using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FlashSaleDB;
using PaymentService.Application.Commands;

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

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePaymentRequest req, CancellationToken ct)
        {
            var id = await _mediator.Send(
                new ProcessPaymentCommand(
                    req.OrderId,
                    req.UserId,
                    req.Amount,
                    req.PaymentMethod,
                    req.CorrelationId
                ),
                ct);

            return CreatedAtAction(nameof(GetByOrder), new { orderId = req.OrderId }, new { paymentId = id });
        }
    }
}
