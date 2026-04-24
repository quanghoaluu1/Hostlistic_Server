using AIService_Application.DTOs;
using Common;

namespace AIService_Application.Interface;

/// <summary>
/// Concurrently fetches and aggregates data from EventService and BookingService
/// into a single, LLM-optimized context object for the post-event summary feature.
/// This service is intentionally free of any LLM/Gemini dependencies.
/// </summary>
public interface IAiDataAggregationService
{
    /// <summary>
    /// Gathers all post-event data for <paramref name="eventId"/> in parallel,
    /// performs key metric calculations, minimizes data for LLM consumption,
    /// and returns the result wrapped in ApiResponse.
    /// Partial service failures are handled gracefully — the DTO will still be
    /// returned, but <see cref="EventExecutiveSummaryDataDto.HasPartialData"/> will be true.
    /// </summary>
    /// <param name="eventId">The event whose summary data is being assembled.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<ApiResponse<EventExecutiveSummaryDataDto>> GetEventSummaryContextAsync(
        Guid eventId,
        CancellationToken ct = default);
}
