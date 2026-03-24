using Microsoft.EntityFrameworkCore;
using NotificationService_Domain.Entities;

namespace NotificationService_Infrastructure.Data;

public class NotificationServiceDbContext : DbContext
{
    public NotificationServiceDbContext(DbContextOptions<NotificationServiceDbContext> options) : base(options)
    {
    }

    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<UserNotification> UserNotifications => Set<UserNotification>();
    public DbSet<EmailCampaign> EmailCampaigns => Set<EmailCampaign>();
    public DbSet<EmailLog> EmailLogs => Set<EmailLog>();
    public DbSet<EventRecipient> EventRecipients => Set<EventRecipient>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Notification configuration
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.OwnsOne(e => e.TargetData, td =>
            {
                td.ToJson();
            });

            entity.HasMany(e => e.UserNotifications)
                .WithOne()
                .HasForeignKey(un => un.NotificationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // UserNotification configuration
        modelBuilder.Entity<UserNotification>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        // EmailCampaign configuration
        modelBuilder.Entity<EmailCampaign>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.OwnsOne(e => e.TargetFilter, tf =>
            {
                tf.ToJson();
            });

            entity.HasMany(e => e.EmailLogs)
                .WithOne()
                .HasForeignKey(el => el.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // EmailLog configuration
        modelBuilder.Entity<EmailLog>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
        
        modelBuilder.Entity<EventRecipient>(entity =>
        {
            entity.HasKey(e => e.Id);
 
            // Unique index for idempotent upserts
            entity.HasIndex(e => new { e.EventId, e.UserId, e.TicketTypeId })
                .IsUnique()
                .HasFilter("\"TicketTypeId\" IS NOT NULL");
 
            // Covering index for recipient resolution queries
            entity.HasIndex(e => new { e.EventId, e.IsCheckedIn });
 
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.FullName).HasMaxLength(256);
            entity.Property(e => e.TicketTypeName).HasMaxLength(128);
        });
    }
}
