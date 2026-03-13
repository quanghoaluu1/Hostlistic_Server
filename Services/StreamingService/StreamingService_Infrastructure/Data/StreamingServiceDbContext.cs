using Microsoft.EntityFrameworkCore;
using StreamingService_Domain.Entities;

namespace StreamingService_Infrastructure.Data;

public class StreamingServiceDbContext : DbContext
{
    public StreamingServiceDbContext(DbContextOptions<StreamingServiceDbContext> options) : base(options)
    {
    }

    public DbSet<StreamRoom> StreamRooms => Set<StreamRoom>();
    public DbSet<StreamParticipant> StreamParticipants => Set<StreamParticipant>();
    public DbSet<StreamRecording> StreamRecordings => Set<StreamRecording>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // StreamRoom configuration
        modelBuilder.Entity<StreamRoom>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.HasMany(e => e.Participants)
                .WithOne(p => p.StreamRoom)
                .HasForeignKey(p => p.StreamRoomId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Recordings)
                .WithOne(r => r.StreamRoom)
                .HasForeignKey(r => r.StreamRoomId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // StreamParticipant configuration
        modelBuilder.Entity<StreamParticipant>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Role)
                .HasConversion<string>()
                .HasMaxLength(50);
        });

        // StreamRecording configuration
        modelBuilder.Entity<StreamRecording>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.Property(e => e.FileSizeBytes);
        });
    }
}
