using Microsoft.EntityFrameworkCore;
using StreamingService_Domain.Entities;

namespace StreamingService_Application.Interfaces;

public interface IStreamingServiceDbContext
{
    DbSet<StreamRoom> StreamRooms { get; }
    DbSet<StreamParticipant> StreamParticipants { get; }
    DbSet<StreamRecording> StreamRecordings { get; }

    DbSet<TEntity> Set<TEntity>() where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
