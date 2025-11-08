using FlashSaleDB;
using FlashSaleDB.Entities;
using Microsoft.EntityFrameworkCore;
using PaymentService.Application.Contracts;

namespace PaymentService.Infrastructure.Persistence.Repositories
{
    internal sealed class PaymentRepository : IPaymentRepository
    {
        private readonly FlashSaleDbContext _ctx;
        public PaymentRepository(FlashSaleDbContext ctx) => _ctx = ctx;

        public Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
            => _ctx.Payment.FirstOrDefaultAsync(x => x.OrderId == orderId, ct);

        public Task AddAsync(Payment payment, CancellationToken ct = default)
            => _ctx.Payment.AddAsync(payment, ct).AsTask();

        public Task SaveChangesAsync(CancellationToken ct = default)
            => _ctx.SaveChangesAsync(ct);

        public Task<bool> OrderExistsAsync(Guid orderId, CancellationToken ct = default)
            => _ctx.Order.AnyAsync(o => o.Id == orderId, ct);
    }
}
