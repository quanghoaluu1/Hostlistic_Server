using BookingService_Application.DTOs;
using BookingService_Application.DTOs.PayOs;
using PayOS.Models.Webhooks;

namespace BookingService_Application.Interfaces;

public interface IPayOsService
{

    Task<PayOsCheckoutResult?> CreatePaymentLinkAsync(CreatePayOsPaymentRequest request);
    Task<WebhookData?> VerifyWebhookAsync(PayOsWebhookPayload payload);
    Task<PayOsWebhookResult?> HandleWebhookAsync(string rawJson);
    Task<PayOsPaymentStatusResult?> GetPaymentStatusAsync(long orderCode);
    Task<bool> CancelPaymentLinkAsync(long orderCode, string? reason = null);

    Task<PayoutResult> CreatePayoutAsync(string referenceId, long amount,
        string description,
        string toBin,
        string toAccountNumber,
        CancellationToken ct = default);
}