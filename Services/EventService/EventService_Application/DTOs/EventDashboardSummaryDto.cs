namespace EventService_Application.DTOs;

/// <summary>
/// Lightweight summary statistics for the organizer dashboard BFF endpoint.
/// All counts are scoped to a single event and computed by the EventService,
/// which owns every related aggregate root.
/// </summary>
public sealed record EventDashboardSummaryDto(
    Guid    EventId,

    // ── Core identity ───────────────────────────────────────────────────────
    string  EventTitle,
    string  EventStatus,

    // ── Schedule ────────────────────────────────────────────────────────────
    DateTime? StartDate,
    DateTime? EndDate,

    // ── Aggregate counts ────────────────────────────────────────────────────
    int TotalSessions,
    int TotalTracks,
    int TotalLineups,       // talent/speaker appearances
    int TotalTalents,       // distinct talent records linked to this event
    int TotalTicketTypes,
    int TotalSponsors,
    int TotalTeamMembers,
    int TotalEventDays,
    int TotalSurveyForms,
    int TotalFeedbacks
);
