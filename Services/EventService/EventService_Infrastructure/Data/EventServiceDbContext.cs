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
    public DbSet<Feedback> Feedbacks => Set<Feedback>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Event configuration
        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne<EventType>()
                .WithMany(et => et.Events)
                .HasForeignKey(e => e.EventTypeId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne<Venue>()
                .WithMany(v => v.Events)
                .HasForeignKey(e => e.VenueId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.EventTeamMembers)
                .WithOne()
                .HasForeignKey(etm => etm.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Tracks)
                .WithOne()
                .HasForeignKey(t => t.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Sessions)
                .WithOne()
                .HasForeignKey(s => s.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Lineups)
                .WithOne()
                .HasForeignKey(l => l.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.TicketTypes)
                .WithOne()
                .HasForeignKey(tt => tt.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Sponsors)
                .WithOne()
                .HasForeignKey(s => s.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.CheckIns)
                .WithOne()
                .HasForeignKey(c => c.EventId)
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
            
            entity.Property(e => e.Permissions)
                .HasColumnType("jsonb");
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
                .WithOne()
                .HasForeignKey(s => s.VenueId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Track configuration
        modelBuilder.Entity<Track>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasMany(e => e.Sessions)
                .WithOne()
                .HasForeignKey(s => s.TrackId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Session configuration
        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasMany(e => e.Polls)
                .WithOne()
                .HasForeignKey(p => p.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.QaQuestions)
                .WithOne()
                .HasForeignKey(q => q.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.SessionBookings)
                .WithOne()
                .HasForeignKey(sb => sb.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Lineups)
                .WithOne()
                .HasForeignKey(l => l.SessionId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.CheckIns)
                .WithOne()
                .HasForeignKey(c => c.SessionId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // SessionBooking configuration
        modelBuilder.Entity<SessionBooking>(entity =>
        {
            entity.HasKey(e => e.Id);
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
                .WithOne()
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
                .WithOne()
                .HasForeignKey(si => si.SponsorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // SponsorTier configuration
        modelBuilder.Entity<SponsorTier>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasMany(e => e.Sponsors)
                .WithOne()
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
                .HasColumnType("jsonb");
            
            entity.Property(e => e.CorrectAnswers)
                .HasColumnType("jsonb");

            entity.HasMany(e => e.PollResponses)
                .WithOne()
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
                .WithOne()
                .HasForeignKey(v => v.QaQuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // QaVote configuration - Composite Primary Key
        modelBuilder.Entity<QaVote>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.QaQuestionId });
        });

        // Feedback configuration
        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
    }
}
