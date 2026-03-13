using BookingService_Domain.Enum;
using Common;

namespace BookingService_Domain.Entities;

public class Wallet : BaseClass
{
    public Guid UserId { get; set; }
    
    public decimal Balance { get; set; }
    public decimal PendingBalance { get; set; }
    
    public string Currency { get; set; } = "VND";
    public WalletStatus Status { get; set; }
    
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<PayoutRequest> PayoutRequests { get; set; } = new List<PayoutRequest>();
}