using BookingService_Domain.Entities;

namespace BookingService_Domain.Interfaces;

public interface IPaymentMethodRepository
{
    Task<PaymentMethod?> GetPaymentMethodByIdAsync(Guid paymentMethodId);
    Task<IEnumerable<PaymentMethod>> GetActivePaymentMethodsAsync();
    Task<IEnumerable<PaymentMethod>> GetAllPaymentMethodsAsync();
    Task<PaymentMethod?> GetPaymentMethodByCodeAsync(string code);
    Task<PaymentMethod> AddPaymentMethodAsync(PaymentMethod paymentMethod);
    Task<PaymentMethod> UpdatePaymentMethodAsync(PaymentMethod paymentMethod);
    Task<bool> DeletePaymentMethodAsync(Guid paymentMethodId);
    Task<bool> PaymentMethodExistsAsync(Guid paymentMethodId);
    Task<bool> PaymentMethodCodeExistsAsync(string code);
    Task SaveChangesAsync();
}