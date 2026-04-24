namespace BookingService_Domain.Interfaces;

/// <summary>
/// Abstracts the database unit-of-work boundary so the Application layer
/// can coordinate atomic commits without taking a hard dependency on EF Core
/// or any specific Infrastructure type.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    /// <summary>Persists all pending changes in the current DbContext session.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    /// <summary>Begins a database transaction and returns a scope handle.</summary>
    Task<ITransactionScope> BeginTransactionAsync(CancellationToken ct = default);
}

/// <summary>
/// Wraps an active database transaction so callers can commit or roll back
/// without a hard EF Core dependency.
/// </summary>
public interface ITransactionScope : IAsyncDisposable
{
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
}
