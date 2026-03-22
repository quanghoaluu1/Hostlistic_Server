namespace BookingService_Application.Interfaces;

public interface IQrCodeService
{
    Task<string> GenerateQrCodeAsync(string ticketCode);
}
