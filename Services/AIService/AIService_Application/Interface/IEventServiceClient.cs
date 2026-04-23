using AIService_Application.DTOs.External;
using AIService_Application.DTOs.Requests;
using AIService_Application.DTOs.Responses;

namespace AIService_Application.Interface;

public interface IEventServiceClient
{
    Task<EventDetailDto?> GetEventByIdAsync(Guid eventId, CancellationToken ct = default);
    Task<LineupDetailDto?> GetEventLineupAsync(Guid eventId, CancellationToken ct = default);

    // ── Post-Event Summary ────────────────────────────────────────────────────

    /// <summary>
    /// Returns Q&amp;A and poll engagement analytics aggregated across all sessions.
    /// Maps to GET /api/events/{eventId}/engagement/analytics
    /// </summary>
    Task<ExternalEngagementAnalyticsDto?> GetEngagementAnalyticsAsync(Guid eventId, CancellationToken ct = default);

    /// <summary>
    /// Returns all attendee feedback (ratings + comments) for an event.
    /// Maps to GET /api/feedback/event/{eventId}
    /// </summary>
    Task<IReadOnlyList<ExternalFeedbackDto>> GetFeedbackAsync(Guid eventId, CancellationToken ct = default);

    /// <summary>
    /// Returns the list of all surveys for an event (organizer view).
    /// Maps to GET /api/events/{eventId}/surveys
    /// </summary>
    Task<IReadOnlyList<ExternalSurveyFormDto>> GetSurveysAsync(Guid eventId, CancellationToken ct = default);

    /// <summary>
    /// Returns the aggregated summary (option counts, text responses) for a single survey.
    /// Maps to GET /api/events/{eventId}/surveys/{surveyId}/summary
    /// </summary>
    Task<ExternalSurveySummaryDto?> GetSurveySummaryAsync(Guid eventId, Guid surveyId, CancellationToken ct = default);
}
