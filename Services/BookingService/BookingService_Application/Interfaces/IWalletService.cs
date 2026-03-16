using BookingService_Application.DTOs;
using Common;

namespace BookingService_Application.Interfaces;

public interface IWalletService
{
    Task<ApiResponse<WalletDto>> GetWalletByIdAsync(Guid walletId);
    Task<ApiResponse<WalletDto>> GetWalletByUserIdAsync(Guid userId);
    Task<ApiResponse<WalletDto>> CreateWalletAsync(CreateWalletRequest request);
    Task<ApiResponse<WalletDto>> UpdateWalletBalanceAsync(Guid walletId, UpdateWalletBalanceRequest request);
    Task<ApiResponse<bool>> DeleteWalletAsync(Guid walletId);
}
