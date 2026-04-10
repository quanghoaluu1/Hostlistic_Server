using System.ComponentModel.DataAnnotations.Schema;
using BookingService_Domain.Enum;

namespace BookingService_Domain.Entities;

public class WithdrawalRequest
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid WalletId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    // Bank info snapshot (copied from IdentityService at request time)
    public string BankName { get; set; } = string.Empty;
    public string BankBin { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;

    // Workflow
    public WithdrawalStatus Status { get; set; } = WithdrawalStatus.Pending;
    public string? PayosReferenceId { get; set; }
    public string? PayosPayoutId { get; set; }
    public string? AdminNotes { get; set; }
    public string? RejectionReason { get; set; }

    public Guid? ApprovedByAdminId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey("WalletId")]
    public virtual Wallet Wallet { get; set; } = null!;
}