using BookingService_Application.DTOs;
using BookingService_Application.DTOs.PayOs;
using PayOS.Models.Webhooks;

namespace BookingService_Application.Interfaces;

public interface IPayOsService
{

    Task<PayOsCheckoutResult?> CreatePaymentLinkAsync(CreatePayOsPaymentRequest request);
    Task<WebhookData?> VerifyWebhookAsync(PayOsWebhookPayload payload);
    Task<PayOsPaymentStatusResult?> GetPaymentStatusAsync(long orderCode);
    Task<bool> CancelPaymentLinkAsync(long orderCode, string? reason = null);
}