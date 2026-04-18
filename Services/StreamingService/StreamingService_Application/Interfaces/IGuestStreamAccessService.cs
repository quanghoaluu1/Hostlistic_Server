namespace StreamingService_Application.Interfaces;

public class GuestLiveAttemptStatus
{
    public bool IsBlocked { get; set; }
    public int FailedAttempts { get; set; }
    public int RemainingAttempts { get; set; }
    public DateTime? BlockedUntilUtc { get; set; }
}

public class GuestLiveSession
{
    public Guid SessionId { get; set; }
    public Guid EventId { get; set; }
    public Guid RoomId { get; set; }
    public Guid TicketId { get; set; }
    public string TicketCode { get; set; } = string.Empty;
    public string Identity { get; set; } = string.Empty;
    public string? HolderName { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime LastSeenAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
}

public interface IGuestStreamAccessService
{
    GuestLiveAttemptStatus GetAttemptStatus(Guid eventId, string clientKey);
    GuestLiveAttemptStatus RegisterFailedAttempt(Guid eventId, string clientKey);
    void ResetAttempts(Guid eventId, string clientKey);
    bool TryGetActiveSession(Guid ticketId, out GuestLiveSession? session);
    GuestLiveSession CreateOrReplaceSession(Guid eventId, Guid roomId, GuestLiveTicketValidationDto ticket, string? holderName);
    bool TouchSession(Guid sessionId, out GuestLiveSession? session);
    void ReleaseSession(Guid sessionId);
}
