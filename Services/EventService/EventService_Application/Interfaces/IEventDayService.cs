using Common;
using EventService_Application.DTOs;

namespace EventService_Application.Interfaces;

public interface IEventDayService
{
    Task<ApiResponse<IReadOnlyList<EventDayResponse>>> GetByEventIdAsync(Guid eventId);
    Task<ApiResponse<EventDayResponse>> GetByIdAsync(Guid eventId, Guid dayId);
    Task<ApiResponse<IReadOnlyList<EventDayResponse>>> GenerateDaysAsync(Guid eventId, GenerateEventDaysRequest request);
    Task<ApiResponse<EventDayResponse>> CreateAsync(Guid eventId, CreateEventDayRequest request);
    Task<ApiResponse<EventDayResponse>> UpdateAsync(Guid eventId, Guid dayId, UpdateEventDayRequest request);
    Task<ApiResponse<bool>> DeleteAsync(Guid eventId, Guid dayId);

    /// <summary>
    /// Syncs EventDays to match the given UTC date range.
    /// Adds missing dates, removes orphan dates (unless sessions block them),
    /// and preserves metadata on overlapping dates.
    /// Returns 409 if any sessions are scheduled on dates being removed.
    /// </summary>
    Task<ApiResponse<IReadOnlyList<EventDayResponse>>> SyncDaysAsync(
        Guid eventId, DateTime newStartDateUtc, DateTime newEndDateUtc,
        string? timeZoneId, CancellationToken ct = default);
}
