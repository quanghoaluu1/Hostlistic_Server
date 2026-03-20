using BookingService_Application.DTOs;
using Common;

namespace BookingService_Application.Interfaces;

public interface IPayOsWebhookHandler
{
    Task<ApiResponse<bool>> HandlePaymentSuccessAsync(PayOsVerifiedPaymentData data);
}