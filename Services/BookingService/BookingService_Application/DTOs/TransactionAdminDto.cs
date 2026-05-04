using BookingService_Domain.Enum;
using Common;

namespace BookingService_Application.DTOs;

public sealed record AdminTransactionQueryRequest
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "CreatedAt";
    public SortDirection SortDirection { get; init; } = SortDirection.desc;
    public TransactionStatus? FilterByStatus { get; init; }
    public TransactionType? FilterByType { get; init; }
    public Guid? WalletId { get; init; }
    public Guid? UserId { get; init; }
    public DateTime? FromDateUtc { get; init; }
    public DateTime? ToDateUtc { get; init; }
    public decimal? MinAmount { get; init; }
    public decimal? MaxAmount { get; init; }
}

public sealed record TransactionAdminDto(
    Guid Id,
    Guid WalletId,
    Guid UserId,
    string Type,
    string Status,
    decimal Amount,
    decimal PlatformFee,
    decimal NetAmount,
    decimal BalanceAfter,
    Guid? ReferenceId,
    string? ReferenceType,
    long? OrderCode,
    string? Description,
    DateTime CreatedAt
);
