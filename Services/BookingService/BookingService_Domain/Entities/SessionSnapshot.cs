namespace BookingService_Domain.Entities;

/// <summary>
/// Read-only ECST cache of Session data synced from EventService via RabbitMQ.
/// This table is never written to directly — all mutations come from
/// SessionSyncedEventConsumer and SessionDeletedEventConsumer.
/// </summary>
public class SessionSnapshot
{
    /// <summary>Mirrors the Session.Id from EventService. Not auto-generated.</summary>
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid TrackId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Location { get; set; }
    public int SessionOrder { get; set; }
    public DateTime LastSyncedAt { get; set; }
}
