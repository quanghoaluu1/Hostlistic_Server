using BookingService_Application.DTOs;
using Common;

namespace BookingService_Application.Interfaces;

public interface IAdminTransactionService
{
    Task<ApiResponse<PagedResult<TransactionAdminDto>>> GetTransactionsAsync(
        AdminTransactionQueryRequest request,
        CancellationToken ct = default);
}
