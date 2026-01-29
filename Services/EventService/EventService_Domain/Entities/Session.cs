using EventService_Domain.Enums;

namespace EventService_Domain.Entities;

public class Session
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid VenueId { get; set; }
    public Guid TrackId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int TotalCapacity { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.Scheduled;
    public QaMode QaMode { get; set; }

    public ICollection<Poll> Polls { get; set; } = new List<Poll>();
    public ICollection<QaQuestion> QaQuestions { get; set; } = new List<QaQuestion>();
    public ICollection<SessionBooking> SessionBookings { get; set; } = new List<SessionBooking>();
    public ICollection<Lineup> Lineups { get; set; } = new List<Lineup>();
}