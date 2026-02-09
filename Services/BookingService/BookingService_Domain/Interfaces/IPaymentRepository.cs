using BookingService_Domain.Entities;

namespace BookingService_Domain.Interfaces;

public interface IPaymentRepository
{
    Task<Payment?> GetPaymentByIdAsync(Guid paymentId);
    Task<IEnumerable<Payment>> GetPaymentsByOrderIdAsync(Guid orderId);
    Task<Payment> AddPaymentAsync(Payment payment);
    Task<Payment> UpdatePaymentAsync(Payment payment);
    Task<bool> DeletePaymentAsync(Guid paymentId);
    Task<bool> PaymentExistsAsync(Guid paymentId);
    Task SaveChangesAsync();
}