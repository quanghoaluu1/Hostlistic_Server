using AIService_Application.DTOs.External;

namespace AIService_Application.Interface;

/// <summary>
/// Abstracts all calls from AIService to BookingService.
/// </summary>
public interface IBookingServiceClient
{
    /// <summary>
    /// Returns a high-level summary: total orders, tickets sold, check-ins, and revenue (also broken down by ticket type).
    /// Maps to GET /api/events/{eventId}/attendees/summary
    /// </summary>
    Task<ExternalAttendeeSummaryDto?> GetAttendeeSummaryAsync(Guid eventId, CancellationToken ct = default);

    /// <summary>
    /// Returns all orders for an event, including line-item details needed for revenue calculations.
    /// Maps to GET /api/orders/event/{eventId}
    /// </summary>
    Task<IReadOnlyList<ExternalOrderDto>> GetOrdersAsync(Guid eventId, CancellationToken ct = default);

    /// <summary>
    /// Returns aggregated check-in statistics: overall counts and breakdowns by ticket type and session.
    /// Maps to GET /api/checkin/event/{eventId}/stats
    /// </summary>
    Task<ExternalCheckInStatsDto?> GetCheckInStatsAsync(Guid eventId, CancellationToken ct = default);
}
