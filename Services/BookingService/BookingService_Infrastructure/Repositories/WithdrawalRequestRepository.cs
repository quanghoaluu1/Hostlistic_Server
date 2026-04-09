using BookingService_Domain.Entities;
using BookingService_Domain.Enum;
using BookingService_Domain.Interfaces;
using BookingService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingService_Infrastructure.Repositories;

public class WithdrawalRequestRepository : IWithdrawalRequestRepository
{
    private readonly BookingServiceDbContext _context;

    public WithdrawalRequestRepository(BookingServiceDbContext context)
    {
        _context = context;
    }

    public async Task<WithdrawalRequest?> GetWithdrawalRequestByIdAsync(Guid withdrawalRequestId)
    {
        return await _context.WithdrawalRequests
            .Include(wr => wr.Wallet)
            .FirstOrDefaultAsync(wr => wr.Id == withdrawalRequestId);
    }
    public async Task<bool> IsUserHasPendingWithdrawalRequest(Guid userId)
    {
        return await _context.WithdrawalRequests.AsNoTracking().AnyAsync(wr => wr.UserId == userId && wr.Status == WithdrawalStatus.Pending);
    }
    public async Task<IEnumerable<WithdrawalRequest>> GetWithdrawalRequestsByUserIdAsync(Guid userId)
    {
        return await _context.WithdrawalRequests
            .AsNoTracking()
            .Where(wr => wr.UserId == userId)
            .OrderByDescending(wr => wr.CreatedAt)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<WithdrawalRequest>> GetWithdrawalRequestsByWalletIdAsync(Guid walletId)
    {
        return await _context.WithdrawalRequests
            .Where(wr => wr.WalletId == walletId)
            .OrderByDescending(wr => wr.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<WithdrawalRequest>> GetWithdrawalRequestsByStatusAsync(WithdrawalStatus status)
    {
        return await _context.WithdrawalRequests
            .Where(wr => wr.Status == status)
            .OrderByDescending(wr => wr.CreatedAt)
            .ToListAsync();
    }

    public async Task<WithdrawalRequest> AddWithdrawalRequestAsync(WithdrawalRequest withdrawalRequest)
    {
        withdrawalRequest.Id = Guid.NewGuid();
        withdrawalRequest.Status = WithdrawalStatus.Pending;
        withdrawalRequest.CreatedAt = DateTime.UtcNow;
        await _context.WithdrawalRequests.AddAsync(withdrawalRequest);
        return withdrawalRequest;
    }

    public Task<WithdrawalRequest> UpdateWithdrawalRequestAsync(WithdrawalRequest withdrawalRequest)
    {
        _context.WithdrawalRequests.Update(withdrawalRequest);
        return Task.FromResult(withdrawalRequest);
    }

    public async Task<bool> DeleteWithdrawalRequestAsync(Guid withdrawalRequestId)
    {
        var withdrawalRequest = await _context.WithdrawalRequests.FindAsync(withdrawalRequestId);
        if (withdrawalRequest == null)
            return false;

        _context.WithdrawalRequests.Remove(withdrawalRequest);
        return true;
    }

    public async Task<bool> WithdrawalRequestExistsAsync(Guid withdrawalRequestId)
    {
        return await _context.WithdrawalRequests.AnyAsync(wr => wr.Id == withdrawalRequestId);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
