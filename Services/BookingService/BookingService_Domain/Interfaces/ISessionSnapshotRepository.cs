using BookingService_Domain.Entities;

namespace BookingService_Domain.Interfaces;

public interface ISessionSnapshotRepository
{
    Task UpsertAsync(SessionSnapshot snapshot);
    /// <returns>True if a row was found and deleted; false if already absent.</returns>
    Task<bool> DeleteAsync(Guid sessionId);
}
