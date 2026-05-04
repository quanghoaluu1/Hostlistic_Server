using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using BookingService_Domain.Interfaces;
using Common;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BookingService_Application.Services;

public sealed class AdminTransactionService(
    ITransactionRepository transactionRepository,
    IValidator<AdminTransactionQueryRequest> validator
) : IAdminTransactionService
{
    private static readonly HashSet<string> AllowedSortBy = new(StringComparer.OrdinalIgnoreCase)
    {
        "CreatedAt",
        "Amount",
        "NetAmount",
        "PlatformFee",
        "BalanceAfter",
        "Status",
        "Type"
    };

    public async Task<ApiResponse<PagedResult<TransactionAdminDto>>> GetTransactionsAsync(
        AdminTransactionQueryRequest request,
        CancellationToken ct = default)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .Select(x => x.ErrorMessage)
                .Distinct()
                .ToList();

            return ApiResponse<PagedResult<TransactionAdminDto>>.FailWithErrors(
                400,
                "Invalid query parameters.",
                errors);
        }

        var query = transactionRepository.GetQueryable()
            .AsNoTracking();

        if (request.FilterByStatus.HasValue)
        {
            query = query.Where(x => x.Status == request.FilterByStatus.Value);
        }

        if (request.FilterByType.HasValue)
        {
            query = query.Where(x => x.Type == request.FilterByType.Value);
        }

        if (request.WalletId.HasValue)
        {
            query = query.Where(x => x.WalletId == request.WalletId.Value);
        }

        if (request.UserId.HasValue)
        {
            query = query.Where(x => x.Wallet.UserId == request.UserId.Value);
        }

        if (request.FromDateUtc.HasValue)
        {
            query = query.Where(x => x.CreatedAt >= request.FromDateUtc.Value);
        }

        if (request.ToDateUtc.HasValue)
        {
            query = query.Where(x => x.CreatedAt <= request.ToDateUtc.Value);
        }

        if (request.MinAmount.HasValue)
        {
            query = query.Where(x => x.Amount >= request.MinAmount.Value);
        }

        if (request.MaxAmount.HasValue)
        {
            query = query.Where(x => x.Amount <= request.MaxAmount.Value);
        }

        var sortExpression = request.SortDirection == SortDirection.desc
            ? $"-{request.SortBy}"
            : request.SortBy;

        query = query.ApplySorting(sortExpression, AllowedSortBy);

        var projected = query.Select(x => new TransactionAdminDto(
            x.Id,
            x.WalletId,
            x.Wallet.UserId,
            x.Type,
            x.Status,
            x.Amount,
            x.PlatformFee,
            x.NetAmount,
            x.BalanceAfter,
            x.ReferenceId,
            x.ReferenceType,
            x.OrderCode,
            x.Description,
            x.CreatedAt
        ));

        var paged = await projected.ToPagedResultAsync(request.PageNumber, request.PageSize);

        return ApiResponse<PagedResult<TransactionAdminDto>>.Success(
            200,
            "Transactions retrieved successfully.",
            paged);
    }
}
