using Common;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using EventService_Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EventService_Application.Services;

/// <summary>
/// Fetches aggregated statistics for the organizer dashboard BFF endpoint.
///
/// EF Core thread-safety strategy:
///   A single LINQ projection query is issued via <see cref="IEventRepository.GetQueryable"/>.
///   EF Core translates the inline sub-selects (Count() inside Select) into a single SQL
///   statement with correlated COUNT sub-queries — one round-trip, fully thread-safe,
///   no <c>Task.WhenAll</c> / concurrent DbContext access.
/// </summary>
public sealed class EventDashboardService(
    IEventRepository eventRepository,
    ILogger<EventDashboardService> logger) : IEventDashboardService
{
    public async Task<ApiResponse<EventDashboardSummaryDto>> GetEventDashboardSummaryAsync(
        Guid eventId,
        CancellationToken ct = default)
    {
        try
        {
            // Single SQL round-trip:  SELECT …, (SELECT COUNT(*) FROM Sessions WHERE …), …
            // EF Core translates inline Count() inside Select into correlated sub-queries.
            // The query is scoped to one event, so all child tables are filtered by EventId.
            var summary = await eventRepository
                .GetQueryable()
                .Where(e => e.Id == eventId)
                .Select(e => new EventDashboardSummaryDto(
                    e.Id,
                    e.Title ?? string.Empty,
                    e.EventStatus.ToString(),
                    e.StartDate,
                    e.EndDate,

                    // Sessions owned directly by the event (track-level + standalone)
                    e.Sessions.Count,

                    // Tracks
                    e.Tracks.Count,

                    // Lineup appearances (one talent can appear in multiple sessions)
                    e.Lineups.Count,

                    // Distinct talents via Lineup join — correlated COUNT DISTINCT
                    e.Lineups.Select(l => l.TalentId).Distinct().Count(),

                    // Ticket types
                    e.TicketTypes.Count,

                    // Sponsors
                    e.Sponsors.Count,

                    // Team members (organizer + staff)
                    e.EventTeamMembers.Count,

                    // Event days (multi-day schedule entries)
                    e.EventDays.Count,

                    // Survey forms
                    e.SurveyForms.Count,

                    // Attendee feedback submissions
                    e.Feedbacks.Count
                ))
                .FirstOrDefaultAsync(ct);

            if (summary is null)
            {
                logger.LogWarning(
                    "Dashboard summary requested for non-existent event {EventId}.", eventId);
                return ApiResponse<EventDashboardSummaryDto>.Fail(404, "Event not found.");
            }

            return ApiResponse<EventDashboardSummaryDto>.Success(
                200, "Dashboard summary retrieved successfully.", summary);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Unexpected error while building dashboard summary for event {EventId}.", eventId);
            return ApiResponse<EventDashboardSummaryDto>.Fail(
                500, "An unexpected error occurred while retrieving the dashboard summary.");
        }
    }
}
