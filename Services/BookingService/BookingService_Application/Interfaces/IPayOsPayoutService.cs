using BookingService_Application.DTOs.PayOs;

namespace BookingService_Application.Interfaces;

public interface IPayOsPayoutService
{
    Task<PayoutResult> CreatePayoutAsync(string referenceId, long amount,
        string description,
        string toBin,
        string toAccountNumber,
        CancellationToken ct = default);
}