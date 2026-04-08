using System.Text.Json;
using EventService_Domain;
using EventService_Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventService_Infrastructure.Data;

public class EventServiceDbContext : DbContext
{
    public EventServiceDbContext(DbContextOptions<EventServiceDbContext> options) : base(options)
    {
    }

    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventType> EventTypes => Set<EventType>();
    public DbSet<EventTeamMember> EventTeamMembers => Set<EventTeamMember>();
    public DbSet<EventTemplate> EventTemplates => Set<EventTemplate>();
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<Track> Tracks => Set<Track>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<SessionBooking> SessionBookings => Set<SessionBooking>();
    public DbSet<Lineup> Lineups => Set<Lineup>();
    public DbSet<Talent> Talents => Set<Talent>();
    public DbSet<TicketType> TicketTypes => Set<TicketType>();
    public DbSet<Sponsor> Sponsors => Set<Sponsor>();
    public DbSet<SponsorTier> SponsorTiers => Set<SponsorTier>();
    public DbSet<SponsorInteraction> SponsorInteractions => Set<SponsorInteraction>();
    public DbSet<CheckIn> CheckIns => Set<CheckIn>();
    public DbSet<Poll> Polls => Set<Poll>();
    public DbSet<PollResponse> PollResponses => Set<PollResponse>();
    public DbSet<QaQuestion> QaQuestions => Set<QaQuestion>();
    public DbSet<QaVote> QaVotes => Set<QaVote>();
    public DbSet<SessionEngagementRestriction> SessionEngagementRestrictions => Set<SessionEngagementRestriction>();
    public DbSet<Feedback> Feedbacks => Set<Feedback>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Event configuration
        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<RichTextContent>(v, (JsonSerializerOptions)null)
                );
            entity.HasOne(e => e.EventType)
                .WithMany(et => et.Events)
                .HasForeignKey(e => e.EventTypeId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.Property(e => e.EventMode)
                .HasConversion<string>()
                    .HasMaxLength(50);
            entity.HasMany(e => e.Venues)
                .WithOne(v => v.Event)
                .HasForeignKey(v => v.EventId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(e => e.EventTeamMembers)
                .WithOne(etm => etm.Event)
                .HasForeignKey(etm => etm.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Tracks)
                .WithOne(t => t.Event)
                .HasForeignKey(t => t.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Sessions)
                .WithOne(s => s.Event)
                .HasForeignKey(s => s.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Lineups)
                .WithOne(l => l.Event)
                .HasForeignKey(l => l.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.TicketTypes)
                .WithOne(tt => tt.Event)
                .HasForeignKey(tt => tt.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Sponsors)
                .WithOne(s => s.Event)
                .HasForeignKey(s => s.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.CheckIns)
                .WithOne(c => c.Event)
                .HasForeignKey(c => c.EventId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(e => e.Feedbacks)
                .WithOne(f => f.Event)
                .HasForeignKey(f => f.EventId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(e => e.SponsorTiers)
                .WithOne(st => st.Event)
                .HasForeignKey(st => st.EventId)
                .OnDelete(DeleteBehavior.Cascade);
            
        });

        // EventType configuration
        modelBuilder.Entity<EventType>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        // EventTeamMember configuration
        modelBuilder.Entity<EventTeamMember>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Role).HasConversion<int>().HasColumnType("integer").IsRequired();

            entity.Property(e => e.Permissions)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, bool>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, bool>());

            entity.Property(e => e.InviteToken).HasMaxLength(64);
            entity.Property(e => e.UserFullName).HasMaxLength(200);
            entity.Property(e => e.UserEmail).HasMaxLength(200);

            // Partial unique index for fast token lookup (NULL values are excluded)
            entity.HasIndex(e => e.InviteToken)
                .IsUnique()
                .HasFilter("\"InviteToken\" IS NOT NULL")
                .HasDatabaseName("IX_EventTeamMembers_InviteToken");
        });

        // EventTemplate configuration
        modelBuilder.Entity<EventTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.OwnsOne(e => e.Config, config =>
            {
                config.ToJson("Config");
                config.OwnsMany(c => c.DefaultTickets);
            });
        });

        // Venue configuration
        modelBuilder.Entity<Venue>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasMany(e => e.Sessions)
                .WithOne(s => s.Venue)
                .HasForeignKey(s => s.VenueId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.Property(v => v.Name).IsRequired().HasMaxLength(200);
            entity.Property(v => v.Description).HasMaxLength(1000);
            entity.HasIndex(v => new { v.EventId, v.Name }).IsUnique();
        });

        // Track configuration
        modelBuilder.Entity<Track>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasMany(e => e.Sessions)
                .WithOne(s => s.Track)
                .HasForeignKey(s => s.TrackId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.EventId, e.SortOrder });
        });

        // Session configuration
        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasMany(e => e.Polls)
                .WithOne(p => p.Session)
                .HasForeignKey(p => p.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.QaQuestions)
                .WithOne(q => q.Session)
                .HasForeignKey(q => q.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.SessionBookings)
                .WithOne(sb => sb.Session)
                .HasForeignKey(sb => sb.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Lineups)
                .WithOne(l => l.Session)
                .HasForeignKey(l => l.SessionId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.CheckIns)
                .WithOne(c => c.Session)
                .HasForeignKey(c => c.SessionId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasMany(e => e.Feedbacks)
                .WithOne(f => f.Session)
                .HasForeignKey(f => f.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(e => e.TicketTypes)
                .WithOne(tt => tt.Session)
                .HasForeignKey(tt => tt.SessionId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasIndex(e => new { e.TrackId, e.StartTime, e.EndTime })
                .HasDatabaseName("IX_Sessions_TrackId_TimeRange");
            entity.HasIndex(e => new { e.VenueId, e.StartTime, e.EndTime })
                .HasDatabaseName("IX_Sessions_VenueId_TimeRange")
                .HasFilter("\"VenueId\" IS NOT NULL"); // Partial index — skip online sessions
            entity.HasIndex(e => new { e.EventId, e.StartTime, e.SortOrder })
                .HasDatabaseName("IX_Sessions_EventId_Ordering");
        });

        // SessionBooking configuration
        modelBuilder.Entity<SessionBooking>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.SessionId })
                .IsUnique()
                .HasDatabaseName("IX_SessionBookings_UserId_SessionId");
            entity.HasIndex(e => new { e.SessionId, e.Status })
                .HasDatabaseName("IX_SessionBookings_SessionId_Status");
        });

        // Lineup configuration
        modelBuilder.Entity<Lineup>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        // Talent configuration
        modelBuilder.Entity<Talent>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasMany(e => e.Lineups)
                .WithOne(l => l.Talent)
                .HasForeignKey(l => l.TalentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TicketType configuration
        modelBuilder.Entity<TicketType>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Price)
                .HasPrecision(18, 2);
        });

        // Sponsor configuration
        modelBuilder.Entity<Sponsor>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasMany(e => e.SponsorInteractions)
                .WithOne(si => si.Sponsor)
                .HasForeignKey(si => si.SponsorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // SponsorTier configuration
        modelBuilder.Entity<SponsorTier>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasMany(e => e.Sponsors)
                .WithOne(s => s.Tier)
                .HasForeignKey(s => s.TierId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // SponsorInteraction configuration
        modelBuilder.Entity<SponsorInteraction>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        // CheckIn configuration
        modelBuilder.Entity<CheckIn>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        // Poll configuration
        modelBuilder.Entity<Poll>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Options)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => string.IsNullOrWhiteSpace(v)
                        ? new List<PollOption>()
                        : JsonSerializer.Deserialize<List<PollOption>>(v, (JsonSerializerOptions?)null) ?? new List<PollOption>()
                );
            
            entity.Property(e => e.CorrectAnswers)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => string.IsNullOrWhiteSpace(v)
                        ? new List<int>()
                        : JsonSerializer.Deserialize<List<int>>(v, (JsonSerializerOptions?)null) ?? new List<int>()
                );

            entity.HasMany(e => e.PollResponses)
                .WithOne(pr => pr.Poll)
                .HasForeignKey(pr => pr.PollId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // PollResponse configuration
        modelBuilder.Entity<PollResponse>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        // QaQuestion configuration
        modelBuilder.Entity<QaQuestion>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasMany(e => e.Votes)
                .WithOne(v => v.QaQuestion)
                .HasForeignKey(v => v.QaQuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // QaVote configuration - Composite Primary Key
        modelBuilder.Entity<QaVote>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.QaQuestionId });
        });

        modelBuilder.Entity<SessionEngagementRestriction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.SessionId, e.UserId, e.Scope, e.IsActive })
                .HasDatabaseName("IX_SessionEngagementRestrictions_SessionUserScope");
            entity.Property(e => e.Reason).HasMaxLength(500);
        });

        // Feedback configuration
        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
    }
}
