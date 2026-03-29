using BookingService_Domain.Entities;
using BookingService_Domain.Interfaces;
using BookingService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingService_Infrastructure.Repositories;

public class SessionSnapshotRepository(BookingServiceDbContext dbContext) : ISessionSnapshotRepository
{
    public async Task UpsertAsync(SessionSnapshot snapshot)
    {
        var existing = await dbContext.SessionSnapshots.FindAsync(snapshot.Id);
        if (existing is null)
        {
            dbContext.SessionSnapshots.Add(snapshot);
        }
        else
        {
            existing.EventId = snapshot.EventId;
            existing.TrackId = snapshot.TrackId;
            existing.Title = snapshot.Title;
            existing.StartTime = snapshot.StartTime;
            existing.EndTime = snapshot.EndTime;
            existing.Location = snapshot.Location;
            existing.SessionOrder = snapshot.SessionOrder;
            existing.LastSyncedAt = snapshot.LastSyncedAt;
        }
        await dbContext.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(Guid sessionId)
    {
        var snapshot = await dbContext.SessionSnapshots.FindAsync(sessionId);
        if (snapshot is null)
            return false;

        dbContext.SessionSnapshots.Remove(snapshot);
        await dbContext.SaveChangesAsync();
        return true;
    }
}
