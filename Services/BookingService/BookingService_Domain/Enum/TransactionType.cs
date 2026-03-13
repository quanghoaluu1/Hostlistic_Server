namespace BookingService_Domain.Enum;

public enum TransactionType
{
    EventRevenue,   // Tiền vé bán được (sau khi trừ commission)
    Payout,         // Organizer rút tiền về bank
    Refund,         // Hoàn tiền → trừ lại wallet
    Adjustment      // Admin điều chỉnh thủ công
}