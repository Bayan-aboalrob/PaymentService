using FlashSaleDB;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PaymentService.Application.Queries;

namespace PaymentService.Application.Payments.Handlers
{
    public sealed class GetPaymentByIdHandler
        : IRequestHandler<GetPaymentByIdQuery, PaymentDetailsDto>
    {
        private readonly FlashSaleDbContext _ctx;

        public GetPaymentByIdHandler(FlashSaleDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<PaymentDetailsDto> Handle(GetPaymentByIdQuery request, CancellationToken ct)
        {
            var payment = await _ctx.Payment
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.PaymentId, ct);

            if (payment is null)
                return null!; 

            return new PaymentDetailsDto(
                PaymentId: payment.Id,
                OrderId: payment.OrderId,
                UserId: payment.UserId ?? Guid.Empty,
                Amount: payment.Amount,
                PaymentMethod: payment.PaymentMethod,
                Status: payment.Status,
                CorrelationId: payment.CorrelationId
            );
        }
    }
}
