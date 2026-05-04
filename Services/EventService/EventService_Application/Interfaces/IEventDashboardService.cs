using Common;
using EventService_Application.DTOs;

namespace EventService_Application.Interfaces;

/// <summary>
/// Provides read-only aggregated statistics for the organizer dashboard BFF endpoint.
/// Intentionally segregated from <see cref="IEventService"/> to keep the service interface
/// focused and avoid bloat as more dashboard stats are added over time.
/// </summary>
public interface IEventDashboardService
{
    /// <summary>
    /// Returns lightweight summary statistics for the specified event.
    /// </summary>
    /// <param name="eventId">The event whose statistics are requested.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 + <see cref="EventDashboardSummaryDto"/> on success.<br/>
    /// 404 if the event does not exist.
    /// </returns>
    Task<ApiResponse<EventDashboardSummaryDto>> GetEventDashboardSummaryAsync(
        Guid eventId,
        CancellationToken ct = default);
}
