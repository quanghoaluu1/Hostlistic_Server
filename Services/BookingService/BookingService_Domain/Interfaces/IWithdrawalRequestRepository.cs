using BookingService_Domain.Entities;
using BookingService_Domain.Enum;

namespace BookingService_Domain.Interfaces;

public interface IWithdrawalRequestRepository
{
    Task<WithdrawalRequest?> GetWithdrawalRequestByIdAsync(Guid withdrawalRequestId);
    Task<IEnumerable<WithdrawalRequest>> GetWithdrawalRequestsByUserIdAsync(Guid userId);
    Task<IEnumerable<WithdrawalRequest>> GetWithdrawalRequestsByWalletIdAsync(Guid walletId);
    Task<IEnumerable<WithdrawalRequest>> GetWithdrawalRequestsByStatusAsync(WithdrawalStatus status);
    Task<WithdrawalRequest> AddWithdrawalRequestAsync(WithdrawalRequest withdrawalRequest);
    Task<WithdrawalRequest> UpdateWithdrawalRequestAsync(WithdrawalRequest withdrawalRequest);
    Task<bool> IsUserHasPendingWithdrawalRequest(Guid userId);
    Task<bool> DeleteWithdrawalRequestAsync(Guid withdrawalRequestId);
    Task<bool> WithdrawalRequestExistsAsync(Guid withdrawalRequestId);
    Task SaveChangesAsync();
}
