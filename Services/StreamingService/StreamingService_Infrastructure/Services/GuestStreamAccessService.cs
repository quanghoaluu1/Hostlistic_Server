using System.Collections.Concurrent;
using StreamingService_Application.Interfaces;

namespace StreamingService_Infrastructure.Services;

public class GuestStreamAccessService : IGuestStreamAccessService
{
    private const int AllowedFailedAttempts = 5;
    private static readonly TimeSpan BlockDuration = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan SessionTtl = TimeSpan.FromMinutes(2);

    private readonly ConcurrentDictionary<string, GuestAttemptEntry> _attempts = new();
    private readonly ConcurrentDictionary<Guid, GuestLiveSession> _sessionsById = new();
    private readonly ConcurrentDictionary<Guid, Guid> _activeTicketSessions = new();

    public GuestLiveAttemptStatus GetAttemptStatus(Guid eventId, string clientKey)
    {
        var key = BuildAttemptKey(eventId, clientKey);
        if (!_attempts.TryGetValue(key, out var entry))
        {
            return NewAttemptStatus(0, null);
        }

        if (entry.BlockedUntilUtc.HasValue && entry.BlockedUntilUtc.Value <= DateTime.UtcNow)
        {
            _attempts.TryRemove(key, out _);
            return NewAttemptStatus(0, null);
        }

        return NewAttemptStatus(entry.FailedAttempts, entry.BlockedUntilUtc);
    }

    public GuestLiveAttemptStatus RegisterFailedAttempt(Guid eventId, string clientKey)
    {
        var key = BuildAttemptKey(eventId, clientKey);
        var entry = _attempts.AddOrUpdate(
            key,
            _ => CreateAttemptEntry(1),
            (_, current) =>
            {
                if (current.BlockedUntilUtc.HasValue && current.BlockedUntilUtc.Value > DateTime.UtcNow)
                    return current;

                var nextAttempts = current.FailedAttempts + 1;
                return CreateAttemptEntry(nextAttempts);
            });

        return NewAttemptStatus(entry.FailedAttempts, entry.BlockedUntilUtc);
    }

    public void ResetAttempts(Guid eventId, string clientKey)
    {
        _attempts.TryRemove(BuildAttemptKey(eventId, clientKey), out _);
    }

    public bool TryGetActiveSession(Guid ticketId, out GuestLiveSession? session)
    {
        session = null;

        if (!_activeTicketSessions.TryGetValue(ticketId, out var sessionId))
            return false;

        if (!_sessionsById.TryGetValue(sessionId, out var storedSession))
        {
            _activeTicketSessions.TryRemove(ticketId, out _);
            return false;
        }

        if (IsExpired(storedSession))
        {
            ReleaseSession(storedSession.SessionId);
            return false;
        }

        session = storedSession;
        return true;
    }

    public bool TryGetSession(Guid sessionId, out GuestLiveSession? session)
    {
        session = null;

        if (!_sessionsById.TryGetValue(sessionId, out var storedSession))
            return false;

        if (IsExpired(storedSession))
        {
            ReleaseSession(sessionId);
            return false;
        }

        session = storedSession;
        return true;
    }

    public GuestLiveSession CreateOrReplaceSession(Guid eventId, Guid roomId, GuestLiveTicketValidationDto ticket, string? holderName)
    {
        CleanupExpiredSession(ticket.TicketId);

        var session = new GuestLiveSession
        {
            SessionId = Guid.NewGuid(),
            EventId = eventId,
            RoomId = roomId,
            TicketId = ticket.TicketId,
            TicketCode = ticket.TicketCode,
            Identity = $"guest-{ticket.TicketId:N}-{Guid.NewGuid():N}"[..36],
            HolderName = string.IsNullOrWhiteSpace(holderName) ? ticket.HolderName : holderName.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
            LastSeenAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.Add(SessionTtl)
        };

        _sessionsById[session.SessionId] = session;
        _activeTicketSessions[ticket.TicketId] = session.SessionId;
        return session;
    }

    public bool TouchSession(Guid sessionId, out GuestLiveSession? session)
    {
        session = null;

        if (!_sessionsById.TryGetValue(sessionId, out var existing))
            return false;

        if (IsExpired(existing))
        {
            ReleaseSession(sessionId);
            return false;
        }

        existing.LastSeenAtUtc = DateTime.UtcNow;
        existing.ExpiresAtUtc = DateTime.UtcNow.Add(SessionTtl);
        _sessionsById[sessionId] = existing;
        session = existing;
        return true;
    }

    public void ReleaseSession(Guid sessionId)
    {
        if (!_sessionsById.TryRemove(sessionId, out var session))
            return;

        if (_activeTicketSessions.TryGetValue(session.TicketId, out var activeSessionId) && activeSessionId == sessionId)
            _activeTicketSessions.TryRemove(session.TicketId, out _);
    }

    private void CleanupExpiredSession(Guid ticketId)
    {
        if (!TryGetActiveSession(ticketId, out _))
            return;
    }

    private static bool IsExpired(GuestLiveSession session)
    {
        return session.ExpiresAtUtc <= DateTime.UtcNow;
    }

    private static GuestAttemptEntry CreateAttemptEntry(int failedAttempts)
    {
        DateTime? blockedUntil = failedAttempts > AllowedFailedAttempts ? DateTime.UtcNow.Add(BlockDuration) : null;
        return new GuestAttemptEntry
        {
            FailedAttempts = failedAttempts,
            BlockedUntilUtc = blockedUntil
        };
    }

    private static GuestLiveAttemptStatus NewAttemptStatus(int failedAttempts, DateTime? blockedUntilUtc)
    {
        return new GuestLiveAttemptStatus
        {
            FailedAttempts = failedAttempts,
            RemainingAttempts = Math.Max(0, AllowedFailedAttempts - failedAttempts),
            IsBlocked = blockedUntilUtc.HasValue && blockedUntilUtc.Value > DateTime.UtcNow,
            BlockedUntilUtc = blockedUntilUtc
        };
    }

    private static string BuildAttemptKey(Guid eventId, string clientKey)
    {
        return $"{eventId:N}:{clientKey.Trim()}";
    }

    private sealed class GuestAttemptEntry
    {
        public int FailedAttempts { get; set; }
        public DateTime? BlockedUntilUtc { get; set; }
    }
}
