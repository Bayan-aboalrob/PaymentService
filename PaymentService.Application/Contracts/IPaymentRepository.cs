using FlashSaleDB.Entities;

namespace PaymentService.Application.Contracts
{
    public interface IPaymentRepository
    {
        Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
        Task AddAsync(Payment payment, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
