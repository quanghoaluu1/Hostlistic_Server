namespace BookingService_Domain.Enum;

public enum PostponementStatus
{
    PendingDecision,
    Accepted,
    RefundRequested,
    Refunded,       // Terminal state: refund has been credited to the user's wallet
}
