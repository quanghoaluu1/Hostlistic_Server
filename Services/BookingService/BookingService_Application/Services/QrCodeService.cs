using BookingService_Application.Interfaces;

namespace BookingService_Application.Services;

public class QrCodeService : IQrCodeService
{
    public Task<string> GenerateQrCodeAsync(string ticketCode) =>
        Task.FromResult($"hostlistic://checkin/{ticketCode}");
}
