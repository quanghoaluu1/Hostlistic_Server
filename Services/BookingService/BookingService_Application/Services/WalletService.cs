using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using BookingService_Domain.Entities;
using BookingService_Domain.Enum;
using BookingService_Domain.Interfaces;
using Common;
using Mapster;
using Microsoft.Extensions.Logging;

namespace BookingService_Application.Services;

public class WalletService : IWalletService
{
    private readonly IWalletRepository _walletRepository;
    private readonly ILogger<WalletService> _logger;

    public WalletService(IWalletRepository walletRepository, ILogger<WalletService> logger)
    {
        _walletRepository = walletRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<WalletDto>> GetWalletByIdAsync(Guid walletId)
    {
        try
        {
            var wallet = await _walletRepository.GetWalletByIdAsync(walletId);
            if (wallet == null)
            {
                return ApiResponse<WalletDto>.Fail(404, "Wallet not found.");
            }

            var dto = wallet.Adapt<WalletDto>();
            return ApiResponse<WalletDto>.Success(200, "Wallet retrieved successfully", dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting wallet with id {walletId}", walletId);
            return ApiResponse<WalletDto>.Fail(500, "An error occurred while retrieving the wallet.");
        }
    }

    public async Task<ApiResponse<WalletDto>> GetWalletByUserIdAsync(Guid userId)
    {
        try
        {
            var wallet = await _walletRepository.GetWalletByUserIdAsync(userId);
            if (wallet == null)
            {
                return ApiResponse<WalletDto>.Fail(404, "Wallet not found for this user.");
            }

            var dto = wallet.Adapt<WalletDto>();
            return ApiResponse<WalletDto>.Success(200, "Wallet retrieved successfully", dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting wallet for user {userId}", userId);
            return ApiResponse<WalletDto>.Fail(500, "An error occurred while retrieving the wallet.");
        }
    }

    public async Task<ApiResponse<WalletDto>> CreateWalletAsync(CreateWalletRequest request)
    {
        try
        {
            var existingWallet = await _walletRepository.GetWalletByUserIdAsync(request.UserId);
            if (existingWallet != null)
            {
                return ApiResponse<WalletDto>.Fail(400, "User already has a wallet.");
            }

            var wallet = new Wallet
            {
                UserId = request.UserId,
                Currency = request.Currency ?? "VND",
                Balance = 0,
                PendingBalance = 0,
                Status = WalletStatus.Active
            };

            await _walletRepository.AddWalletAsync(wallet);
            await _walletRepository.SaveChangesAsync();

            var dto = wallet.Adapt<WalletDto>();
            return ApiResponse<WalletDto>.Success(201, "Wallet created successfully.", dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating wallet for user {userId}", request.UserId);
            return ApiResponse<WalletDto>.Fail(500, "An error occurred while creating the wallet.");
        }
    }

    public async Task<ApiResponse<WalletDto>> UpdateWalletBalanceAsync(Guid walletId, UpdateWalletBalanceRequest request)
    {
        try
        {
            var wallet = await _walletRepository.GetWalletByIdAsync(walletId);
            if (wallet == null)
            {
                return ApiResponse<WalletDto>.Fail(404, "Wallet not found.");
            }

            if (wallet.Status != WalletStatus.Active)
            {
                return ApiResponse<WalletDto>.Fail(400, "Wallet is not active.");
            }

            if (request.TransactionType.Equals("Withdraw", StringComparison.OrdinalIgnoreCase))
            {
                if (wallet.Balance < request.Amount)
                {
                    return ApiResponse<WalletDto>.Fail(400, "Insufficient balance.");
                }
                wallet.Balance -= request.Amount;
            }
            else if (request.TransactionType.Equals("Deposit", StringComparison.OrdinalIgnoreCase))
            {
                wallet.Balance += request.Amount;
            }
            else
            {
                return ApiResponse<WalletDto>.Fail(400, "Invalid transaction type. Use 'Deposit' or 'Withdraw'.");
            }

            await _walletRepository.UpdateWalletAsync(wallet);
            await _walletRepository.SaveChangesAsync();

            var dto = wallet.Adapt<WalletDto>();
            return ApiResponse<WalletDto>.Success(200, "Wallet balance updated successfully.", dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating balance for wallet {walletId}", walletId);
            return ApiResponse<WalletDto>.Fail(500, "An error occurred while updating the wallet balance.");
        }
    }

    public async Task<ApiResponse<bool>> DeleteWalletAsync(Guid walletId)
    {
        try
        {
            var result = await _walletRepository.DeleteWalletAsync(walletId);
            if (!result)
            {
                return ApiResponse<bool>.Fail(404, "Wallet not found.");
            }

            await _walletRepository.SaveChangesAsync();
            return ApiResponse<bool>.Success(200, "Wallet deleted successfully.", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting wallet {walletId}", walletId);
            return ApiResponse<bool>.Fail(500, "An error occurred while deleting the wallet.");
        }
    }
}
