using BookingService_Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace BookingService_Infrastructure.Data;

/// <summary>
/// EF Core implementation of <see cref="ITransactionScope"/>.
/// Wraps an <see cref="IDbContextTransaction"/> so Application-layer callers
/// never reference EF Core types directly.
/// </summary>
public sealed class EfTransactionScope(IDbContextTransaction inner) : ITransactionScope
{
    public Task CommitAsync(CancellationToken ct = default)  => inner.CommitAsync(ct);
    public Task RollbackAsync(CancellationToken ct = default) => inner.RollbackAsync(ct);
    public ValueTask DisposeAsync() => inner.DisposeAsync();
}

/// <summary>
/// EF Core implementation of <see cref="IUnitOfWork"/>.
/// Delegates SaveChanges and transaction management to <see cref="BookingServiceDbContext"/>.
/// </summary>
public sealed class EfUnitOfWork(BookingServiceDbContext db) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);

    public async Task<ITransactionScope> BeginTransactionAsync(CancellationToken ct = default)
    {
        var tx = await db.Database.BeginTransactionAsync(ct);
        return new EfTransactionScope(tx);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
