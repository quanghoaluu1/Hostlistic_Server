using BookingService_Domain.Entities;
using BookingService_Domain.Interfaces;
using BookingService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingService_Infrastructure.Repositories;

public class WalletRepository : IWalletRepository
{
    private readonly BookingServiceDbContext _context;

    public WalletRepository(BookingServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Wallet?> GetWalletByIdAsync(Guid walletId)
    {
        return await _context.Wallets
            .FirstOrDefaultAsync(w => w.Id == walletId);
    }

    public async Task<Wallet?> GetWalletByUserIdAsync(Guid userId)
    {
        return await _context.Wallets
            .FirstOrDefaultAsync(w => w.UserId == userId);
    }

    public async Task<Wallet> AddWalletAsync(Wallet wallet)
    {
        wallet.Id = Guid.NewGuid();
        wallet.Status = BookingService_Domain.Enum.WalletStatus.Active;
        wallet.Balance = 0;
        wallet.PendingBalance = 0;
        await _context.Wallets.AddAsync(wallet);
        return wallet;
    }

    public Task<Wallet> UpdateWalletAsync(Wallet wallet)
    {
        wallet.UpdatedAt = DateTime.UtcNow;
        _context.Wallets.Update(wallet);
        return Task.FromResult(wallet);
    }

    public async Task<bool> DeleteWalletAsync(Guid walletId)
    {
        var wallet = await _context.Wallets.FindAsync(walletId);
        if (wallet == null)
            return false;

        _context.Wallets.Remove(wallet);
        return true;
    }

    public async Task<bool> WalletExistsAsync(Guid walletId)
    {
        return await _context.Wallets.AnyAsync(w => w.Id == walletId);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}