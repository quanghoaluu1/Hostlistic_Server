using BookingService_Application.DTOs;
using BookingService_Domain.Enum;
using Common;

namespace BookingService_Application.Interfaces;

public interface IWithdrawalRequestService
{
    Task<ApiResponse<WithdrawalDto>> CreateRequestAsync(Guid userId, CreateWithdrawalRequest request,
        CancellationToken ct = default);

    Task<ApiResponse<List<WithdrawalDto>>> GetMyWithdrawalsAsync(
        Guid userId, CancellationToken ct = default);

    Task<ApiResponse<List<WithdrawalDto>>> GetWithdrawalsByStatusAsync(WithdrawalStatus status,
        CancellationToken ct = default);

    Task<ApiResponse<WithdrawalDto>> ApproveAsync(Guid withdrawalId, Guid adminId, string? notes,
        CancellationToken ct = default);

    Task<ApiResponse<WithdrawalDto>> RejectAsync(
        Guid withdrawalId, Guid adminId, string reason, CancellationToken ct = default);
}