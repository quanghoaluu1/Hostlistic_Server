using BookingService_Application.DTOs;
using BookingService_Application.DTOs.PayOs;
using BookingService_Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PayOS;
using PayOS.Exceptions;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;

namespace BookingService_Application.Services;

public class PayOsService(
    PayOSClient payOsClient,
    IConfiguration configuration,
    ILogger<PayOsService> logger
    ) : IPayOsService
{
    private readonly string _returnBaseUrl = configuration["FrontEndUrl"] ?? "http://localhost:3000/";


    public async Task<PayOsCheckoutResult?> CreatePaymentLinkAsync(CreatePayOsPaymentRequest  request)
    {
        try
        {
            var paymentRequest = new CreatePaymentLinkRequest()
            {
                OrderCode = request.OrderCode,
                Amount = (int)request.Amount, // PayOS chỉ nhận int (VND không lẻ)
                Description = request.Description,
                ReturnUrl = $"{_returnBaseUrl}/payment/success?orderId={request.OrderId}",
                CancelUrl = $"{_returnBaseUrl}/payment/cancel?orderId={request.OrderId}",
                Items = request.Items.Select(i => new PaymentLinkItem
                {
                    Name = i.Name,
                    Quantity = i.Quantity,
                    Price = (int)i.Price
                }).ToList()
            };

            var result = await payOsClient.PaymentRequests.CreateAsync(paymentRequest);
            return new PayOsCheckoutResult()
            {
                CheckoutUrl = result.CheckoutUrl,
                QrCode = result.QrCode,
                PaymentLinkId = result.PaymentLinkId,
            };
        }catch (ApiException ex)
        {
            logger.LogError("PayOS API error for orderCode {OrderCode}: {Code} - {Message}",
                request.OrderCode, ex.ErrorCode, ex.Message);
            return null;
        }
        catch (PayOSException ex)
        {
            logger.LogError(ex, "PayOS error for orderCode {OrderCode}", request.OrderCode);
            return null;
        }
    }
    
    
    public async Task<WebhookData?> VerifyWebhookAsync(PayOsWebhookPayload payload)
    {
        try
        {
            var webhook = new Webhook
            {
                Code = payload.Code,
                Description = payload.Desc,
                Success = payload.Success,
                Data = payload.Data is not null ? new WebhookData
                {
                    OrderCode = payload.Data.OrderCode,
                    Amount = payload.Data.Amount,
                    Description = payload.Data.Description,
                    AccountNumber = payload.Data.AccountNumber,
                    Reference = payload.Data.Reference,
                    TransactionDateTime = payload.Data.TransactionDateTime,
                    Code = payload.Data.Code,
                    Description2 = payload.Data.Desc,
                    CounterAccountBankId = payload.Data.CounterAccountBankId,
                    CounterAccountBankName = payload.Data.CounterAccountBankName,
                    CounterAccountName = payload.Data.CounterAccountName,
                    CounterAccountNumber = payload.Data.CounterAccountNumber
                } : null!,
                Signature = payload.Signature
            };

            // SDK verify signature + trả về parsed WebhookData
            var verifiedData = await payOsClient.Webhooks.VerifyAsync(webhook);
            return verifiedData;
        }
        catch (PayOSException ex)
        {
            logger.LogWarning("PayOS webhook verification failed: {Message}", ex.Message);
            return null; // Signature invalid → reject
        }
    }

    public async Task<PayOsPaymentStatusResult?> GetPaymentStatusAsync(long orderCode)
    {
        try
        {
            var info = await payOsClient.PaymentRequests.GetAsync(orderCode);
            return new PayOsPaymentStatusResult
            {
                OrderCode = info.OrderCode,
                Amount = info.Amount,
                Status = info.Status.ToString(), // PAID, PENDING, CANCELLED, EXPIRED
                TransactionId = info.Transactions?.FirstOrDefault()?.Reference
            };
        }
        catch (ApiException ex)
        {
            logger.LogError("PayOS query failed for orderCode {OrderCode}: {Message}",
                orderCode, ex.Message);
            return null;
        }
    }
    
    public async Task<bool> CancelPaymentLinkAsync(long orderCode, string? reason = null)
    {
        try
        {
            await payOsClient.PaymentRequests.CancelAsync(orderCode, reason);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to cancel PayOS payment for orderCode {OrderCode}", orderCode);
            return false;
        }
    }
}