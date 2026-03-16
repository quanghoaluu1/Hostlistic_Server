using BookingService_Domain.Enum;

namespace BookingService_Application.DTOs;

public class WalletDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public decimal Balance { get; set; }
    public decimal PendingBalance { get; set; }
    public string Currency { get; set; } = "VND";
    public WalletStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateWalletRequest
{
    public Guid UserId { get; set; }
    public string Currency { get; set; } = "VND";
}

public class UpdateWalletBalanceRequest
{
    public decimal Amount { get; set; }
    public string TransactionType { get; set; } // "Deposit" or "Withdraw"
}