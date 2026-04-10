using System.Text.Json;
using BookingService_Application.DTOs;
using BookingService_Application.DTOs.PayOs;
using BookingService_Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PayOS;
using PayOS.Exceptions;
using PayOS.Models.V1;
using PayOS.Models.V1.Payouts;
using PayOS.Models.V2;
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
    
    public async Task<PayOsWebhookResult?> HandleWebhookAsync(string rawJson)
    {
        try
        {
            // Deserialize bằng SDK types — giữ nguyên data gốc
            var webhook = JsonSerializer.Deserialize<Webhook>(rawJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (webhook?.Data is null || webhook.Data.OrderCode == 0)
                return null; // Ping request

            // SDK verify trên đúng object gốc — signature khớp
            var verified = await payOsClient.Webhooks.VerifyAsync(webhook);

            return new PayOsWebhookResult
            {
                IsVerified = true,
                IsSuccess = webhook.Code == "00",
                Data = new PayOsVerifiedPaymentData
                {
                    OrderCode = verified.OrderCode,
                    Amount = verified.Amount,
                    Reference = verified.Reference,
                    TransactionDateTime = verified.TransactionDateTime
                }
            };
        }
        catch (PayOSException ex)
        {
            logger.LogWarning("Webhook verification failed: {Message}", ex.Message);
            return new PayOsWebhookResult { IsVerified = false };
        }
    }

    public async Task<PayoutResult> CreatePayoutAsync(string referenceId, long amount,
        string description,
        string toBin,
        string toAccountNumber,
        CancellationToken ct = default)
    {
        try
        {
            logger.LogInformation("Creating PayOS payout: ref={Ref}, amount={Amount}, bin={Bin}, account=***{Last4}",
                referenceId, amount, toBin,
                toAccountNumber.Length > 4 ? toAccountNumber[^4..] : toAccountNumber);
            var payoutRequest = new PayoutRequest()
            {
                Amount = amount,
                Description = description,
                ReferenceId = referenceId,
                ToAccountNumber = toAccountNumber,
                ToBin = toBin,
                Category = ["payout"]
            };
            var response = await payOsClient.Payouts.CreateAsync(payoutRequest);
            var api = payOsClient.ApiKey;
            logger.LogInformation("Api Key : {ApiKey}", api.Substring(0, 10) + "...");
            logger.LogInformation(
                "PayOS payout created successfully: ref={Ref}, payoutId={PayoutId}",
                referenceId, response?.Id);
            return new PayoutResult(
                IsSuccess: true,
                PayoutId: response?.Id,
                ReferenceId: referenceId,
                ErrorMessage: null
            );

        }
        catch (ApiException ex)
        {
            logger.LogError(ex,
                "PayOS API error creating payout: ref={Ref}, code={Code}, message={Message}",
                referenceId, ex.StatusCode, ex.Message);

            return new PayoutResult(
                IsSuccess: false,
                PayoutId: null,
                ReferenceId: referenceId,
                ErrorMessage: $"PayOS error ({ex.StatusCode}): {ex.Message}"
            );
        }
        catch (PayOSException ex)
        {
            logger.LogError(ex, "PayOS SDK error creating payout: ref={Ref}", referenceId);

            return new PayoutResult(
                IsSuccess: false,
                PayoutId: null,
                ReferenceId: referenceId,
                ErrorMessage: $"PayOS SDK error: {ex.Message}"
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error creating PayOS payout: ref={Ref}", referenceId);

            return new PayoutResult(
                IsSuccess: false,
                PayoutId: null,
                ReferenceId: referenceId,
                ErrorMessage: $"Unexpected error: {ex.Message}"
            );
        }
    }
}