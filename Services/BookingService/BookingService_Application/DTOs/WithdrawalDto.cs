using BookingService_Domain.Enum;

namespace BookingService_Application.DTOs;

public record CreateWithdrawalRequest(decimal Amount);

public record ApproveWithdrawalRequest(string? AdminNotes);

public record RejectWithdrawalRequest(string Reason);

public record WithdrawalDto(
    Guid Id,
    Guid UserId,
    Guid WalletId,
    decimal Amount,
    string BankName,
    string BankBin,
    string AccountNumber,
    string AccountName,
    WithdrawalStatus Status,
    string? PayosReferenceId,
    string? PayosPayoutId,
    string? AdminNotes,
    string? RejectionReason,
    Guid? ApprovedByAdminId,
    DateTime? ApprovedAt,
    DateTime? CompletedAt,
    DateTime CreatedAt
);