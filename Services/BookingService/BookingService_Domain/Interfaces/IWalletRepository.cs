using BookingService_Domain.Entities;

namespace BookingService_Domain.Interfaces;

public interface IWalletRepository
{
    Task<Wallet?> GetWalletByIdAsync(Guid walletId);
    Task<Wallet?> GetWalletByUserIdAsync(Guid userId);
    Task<Wallet> AddWalletAsync(Wallet wallet);
    Task<Wallet> UpdateWalletAsync(Wallet wallet);
    Task<bool> DeleteWalletAsync(Guid walletId);
    Task<bool> WalletExistsAsync(Guid walletId);
    Task SaveChangesAsync();
}