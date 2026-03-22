using BookingService_Application.DTOs;

namespace BookingService_Application.Interfaces;

public interface IPaymentNotifier
{
    Task NotifyPaymentConfirmedAsync(Guid orderId, PaymentConfirmedPayload payload);
    Task NotifyPaymentFailedAsync(Guid orderId, string reason);
}