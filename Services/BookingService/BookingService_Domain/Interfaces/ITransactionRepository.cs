using BookingService_Domain.Entities;
using BookingService_Domain.Enum;

namespace BookingService_Domain.Interfaces;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid id);
    Task<IEnumerable<Transaction>> GetByWalletIdAsync(Guid walletId);
    Task<IEnumerable<Transaction>> GetByReferenceAsync(Guid referenceId, string referenceType);
    Task<IEnumerable<Transaction>> GetByStatusAsync(TransactionStatus status);
    Task<Transaction> AddAsync(Transaction transaction);
    Task SaveChangesAsync();
    Task<List<dynamic>> GetWeeklyTransactionsRawAsync(
    DateTime start,
    DateTime end,
    Guid? walletId = null,
    bool useNetAmount = false);
}
