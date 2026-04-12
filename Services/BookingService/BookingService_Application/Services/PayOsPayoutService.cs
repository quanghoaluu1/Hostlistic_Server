using BookingService_Application.DTOs.PayOs;
using BookingService_Application.Interfaces;
using Google.Apis.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PayOS;
using PayOS.Exceptions;
using PayOS.Models.V1.Payouts;

namespace BookingService_Application.Services;

public class PayOsPayoutService(
    [FromKeyedServices("Payout")] PayOSClient payOsClient,
    ILogger<PayOsPayoutService> logger
    ) : IPayOsPayoutService
{
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