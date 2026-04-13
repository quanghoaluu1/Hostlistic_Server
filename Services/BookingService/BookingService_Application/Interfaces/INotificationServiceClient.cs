using BookingService_Application.DTOs;

namespace BookingService_Application.Interfaces;

public interface INotificationServiceClient
{
    Task<bool> SendTicketPurchaseConfirmationAsync(PurchaseConfirmationRequest request);
    Task<bool> SendHolderTicketDeliveryAsync(HolderTicketDeliveryRequest request);
}

