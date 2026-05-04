namespace BookingService_Application.DTOs;

public class CreateWalletTopUpRequest
{
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
}
