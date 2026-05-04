namespace BookingService_Application.DTOs;

public class PurchaseSubscriptionWithPayOsRequest
{
    public Guid UserId { get; set; }
    public Guid SubscriptionPlanId { get; set; }
}
