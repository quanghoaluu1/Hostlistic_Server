using BookingService_Domain.Entities;
using BookingService_Domain.Enum;
using BookingService_Domain.Interfaces;
using BookingService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingService_Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly BookingServiceDbContext _context;

    public TransactionRepository(BookingServiceDbContext context)
    {
        _context = context;
    }

    public async Task<Transaction?> GetByIdAsync(Guid id)
    {
        return await _context.Transactions
            .Include(t => t.Wallet)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Transaction>> GetByWalletIdAsync(Guid walletId)
    {
        return await _context.Transactions
            .Where(t => t.WalletId == walletId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<Transaction?> GetByReferenceAsync(Guid referenceId, string referenceType)
    {
        return await _context.Transactions
            .FirstOrDefaultAsync(t => t.ReferenceId == referenceId && t.ReferenceType == referenceType);
    }

    public async Task<IEnumerable<Transaction>> GetByStatusAsync(TransactionStatus status)
    {
        return await _context.Transactions
            .Where(t => t.Status == status)
            .ToListAsync();
    }

    public async Task<Transaction> AddAsync(Transaction transaction)
    {
        transaction.Id = Guid.NewGuid();
        transaction.CreatedAt = DateTime.UtcNow;
        await _context.Transactions.AddAsync(transaction);
        return transaction;
    }
    
    public Task<Transaction> UpdateAsync(Transaction transaction)
    {
        _context.Transactions.Update(transaction);
        return Task.FromResult(transaction);
    }
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<List<dynamic>> GetWeeklyTransactionsRawAsync(
    DateTime start,
    DateTime end,
    Guid? walletId = null,          // null = admin
    bool useNetAmount = false)       // organizer = true
    {
        var query = _context.Transactions
            .Where(t =>
                t.Status == TransactionStatus.Completed &&
                t.CreatedAt >= start &&
                t.CreatedAt < end);

        if (walletId.HasValue)
        {
            query = query.Where(t => t.WalletId == walletId.Value);
        }

        return await query
            .GroupBy(t => t.CreatedAt.Date)
            .Select(g => new
            {
                Date = g.Key,

                MoneyIn = g
                    .Where(x => x.Type == TransactionType.PaymentReceived)
                    .Sum(x => useNetAmount ? x.NetAmount : x.Amount),

                MoneyOut = g
                    .Where(x => x.Type == TransactionType.Payout)
                    .Sum(x => x.Amount)
            })
            .ToListAsync<dynamic>();
    }
}
