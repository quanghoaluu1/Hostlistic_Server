using System.ComponentModel.DataAnnotations.Schema;
using BookingService_Domain.Enum;

namespace BookingService_Domain.Entities;

public class Transaction
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal NetAmount { get; set; }
    public decimal BalanceAfter { get; set; }
    
    //Truy vet nguon goc
    public Guid? ReferenceId { get; set; } //OrderId, PayoutRequestId, PaymentId,...
    public string? ReferenceType { get; set; }
    
    public TransactionStatus Status { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("WalletId")]
    public virtual Wallet Wallet { get; set; } = null!;
}