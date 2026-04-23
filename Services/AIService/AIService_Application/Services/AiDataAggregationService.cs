using AIService_Application.DTOs;
using AIService_Application.DTOs.External;
using AIService_Application.Interface;
using Common;
using Microsoft.Extensions.Logging;

namespace AIService_Application.Services;

/// <summary>
/// Aggregates post-event data from EventService and BookingService concurrently,
/// applies metric calculations and LLM-focused data minimization, and returns
/// a single <see cref="EventExecutiveSummaryDataDto"/> ready for prompt injection.
///
/// Design decisions:
///  • Task.WhenAll for maximum parallelism (all four fetches fire at the same time).
///  • Null/empty guards on every external result — a dead upstream never kills the report.
///  • Feedback is split into positive (≥4 ★) and critical (≤3 ★), capped at 5 each to
///    keep the LLM prompt concise and avoid token overflow.
///  • Sessions are ranked by TotalEngagedParticipants (desc) and capped at 5 items.
///  • No LLM / Gemini dependencies here — this class only aggregates data.
/// </summary>
public class AiDataAggregationService(
    IEventServiceClient eventServiceClient,
    IBookingServiceClient bookingServiceClient,
    ILogger<AiDataAggregationService> logger) : IAiDataAggregationService
{
    private const int MaxFeedbackHighlights = 5;
    private const int MaxTopSessions = 5;
    private const int PositiveRatingThreshold = 4; // ≥ 4 stars = positive
    private const int CriticalRatingThreshold = 3; // ≤ 3 stars = critical

    /// <inheritdoc/>
    public async Task<ApiResponse<EventExecutiveSummaryDataDto>> GetEventSummaryContextAsync(
        Guid eventId,
        CancellationToken ct = default)
    {
        logger.LogInformation(
            "[PostEventSummary] Starting parallel data fetch for event {EventId}", eventId);

        // ── 1. Parallel fetch — all four calls fire concurrently ─────────────
        var attendeeSummaryTask    = bookingServiceClient.GetAttendeeSummaryAsync(eventId, ct);
        var ordersTask             = bookingServiceClient.GetOrdersAsync(eventId, ct);
        var checkInStatsTask       = bookingServiceClient.GetCheckInStatsAsync(eventId, ct);
        var engagementAnalyticsTask = eventServiceClient.GetEngagementAnalyticsAsync(eventId, ct);
        var feedbackTask           = eventServiceClient.GetFeedbackAsync(eventId, ct);
        var eventDetailTask        = eventServiceClient.GetEventByIdAsync(eventId, ct);

        await Task.WhenAll(
            attendeeSummaryTask,
            ordersTask,
            checkInStatsTask,
            engagementAnalyticsTask,
            feedbackTask,
            eventDetailTask);

        // Unwrap results — Task<T> is always completed here, no await needed.
        var attendeeSummary    = attendeeSummaryTask.Result;
        var orders             = ordersTask.Result;
        var checkInStats       = checkInStatsTask.Result;
        var engagement         = engagementAnalyticsTask.Result;
        var feedbacks          = feedbackTask.Result;
        var eventDetail        = eventDetailTask.Result;

        // ── 2. Track data quality (partial failure detection) ─────────────────
        var missingDataSources = new List<string>();

        if (eventDetail is null)
            missingDataSources.Add("EventDetail");
        if (attendeeSummary is null)
            missingDataSources.Add("AttendeeSummary");
        if (orders.Count == 0)
            missingDataSources.Add("Orders");
        if (checkInStats is null)
            missingDataSources.Add("CheckInStats");
        if (engagement is null)
            missingDataSources.Add("EngagementAnalytics");
        if (feedbacks.Count == 0)
            missingDataSources.Add("Feedback");

        if (missingDataSources.Count > 0)
        {
            logger.LogWarning(
                "[PostEventSummary] Partial data for event {EventId}. Missing: {Missing}",
                eventId, string.Join(", ", missingDataSources));
        }

        // ── 3. Metric calculations ────────────────────────────────────────────
        var totalTicketsSold = attendeeSummary?.TotalTicketsSold
            ?? SumSoldTickets(orders);

        var totalCheckedIn   = attendeeSummary?.TotalCheckedIn
            ?? checkInStats?.TotalCheckedIn
            ?? 0;

        var checkInRate      = ComputeCheckInRate(totalCheckedIn, totalTicketsSold);
        var totalRevenue     = attendeeSummary?.TotalRevenue ?? ComputeRevenue(orders);

        var byTicketType     = BuildTicketTypeSummary(attendeeSummary, checkInStats);

        // ── 4. Engagement summary ─────────────────────────────────────────────
        var topSessions      = BuildTopSessions(engagement);
        var totalQuestions   = engagement?.TotalQuestions   ?? 0;
        var approvedQuestions = engagement?.ApprovedQuestions ?? 0;
        var totalPollVotes   = engagement?.TotalPollVotes   ?? 0;

        // ── 5. Feedback minimization for LLM ─────────────────────────────────
        var (avgRating, positiveHighlights, criticalHighlights) = ProcessFeedback(feedbacks);

        // ── 6. Assemble the output DTO ────────────────────────────────────────
        var dto = new EventExecutiveSummaryDataDto
        {
            EventId              = eventId,
            EventName            = eventDetail?.Title ?? string.Empty,
            TotalTicketsSold     = totalTicketsSold,
            TotalCheckedIn       = totalCheckedIn,
            CheckInRate          = checkInRate,
            TotalRevenue         = totalRevenue,
            ByTicketType         = byTicketType,
            TotalQuestions       = totalQuestions,
            ApprovedQuestions    = approvedQuestions,
            TotalPollVotes       = totalPollVotes,
            TopEngagedSessions   = topSessions,
            AverageRating        = avgRating,
            TotalFeedbackCount   = feedbacks.Count,
            HighlightPositiveFeedbacks = positiveHighlights,
            HighlightCriticalFeedbacks = criticalHighlights,
            HasPartialData       = missingDataSources.Count > 0,
            MissingDataSources   = missingDataSources,
        };

        logger.LogInformation(
            "[PostEventSummary] Aggregation complete for event {EventId}. " +
            "Sold={Sold}, CheckedIn={CheckedIn}, Revenue={Revenue:F0}, " +
            "AvgRating={Rating}, HasPartialData={Partial}",
            eventId, dto.TotalTicketsSold, dto.TotalCheckedIn,
            dto.TotalRevenue, dto.AverageRating, dto.HasPartialData);

        return ApiResponse<EventExecutiveSummaryDataDto>.Success(
            200, "Event summary context aggregated successfully.", dto);
    }

    // ── Private Calculation Helpers ───────────────────────────────────────────

    /// <summary>
    /// Fallback: sum quantities from confirmed orders when AttendeeSummary is unavailable.
    /// Confirmed = status 1 (mirrors BookingService OrderStatus enum).
    /// </summary>
    private static int SumSoldTickets(IReadOnlyList<ExternalOrderDto> orders)
        => orders
            .Where(o => o.Status == 1) // 1 = Confirmed
            .Sum(o => o.OrderDetails.Sum(d => d.Quantity));

    /// <summary>
    /// Fallback: sum revenue from confirmed orders when AttendeeSummary is unavailable.
    /// </summary>
    private static decimal ComputeRevenue(IReadOnlyList<ExternalOrderDto> orders)
        => orders
            .Where(o => o.Status == 1) // 1 = Confirmed
            .Sum(o => o.OrderDetails.Sum(d => d.TotalPrice));

    /// <summary>
    /// Returns check-in rate as a percentage (0–100), rounded to 1 decimal place.
    /// Safely handles the zero-denominator edge case.
    /// </summary>
    private static double ComputeCheckInRate(int checkedIn, int sold)
        => sold <= 0 ? 0.0 : Math.Round(checkedIn / (double)sold * 100, 1);

    /// <summary>
    /// Prefers the richer AttendeeSummary breakdown; falls back to CheckInStats when absent.
    /// </summary>
    private static List<TicketTypeSummaryItem> BuildTicketTypeSummary(
        ExternalAttendeeSummaryDto? attendeeSummary,
        ExternalCheckInStatsDto? checkInStats)
    {
        if (attendeeSummary?.ByTicketType is { Count: > 0 })
        {
            return attendeeSummary.ByTicketType
                .Select(t => new TicketTypeSummaryItem
                {
                    TicketTypeName = t.TicketTypeName,
                    TicketCount    = t.TicketCount,
                    CheckedInCount = t.CheckedInCount,
                    Revenue        = t.Revenue,
                })
                .ToList();
        }

        // Fallback — CheckInStats only has check-in numbers, no revenue.
        if (checkInStats?.ByTicketType is { Count: > 0 })
        {
            return checkInStats.ByTicketType
                .Select(t => new TicketTypeSummaryItem
                {
                    TicketTypeName = t.TicketTypeName,
                    TicketCount    = t.TotalSold,
                    CheckedInCount = t.CheckedIn,
                    Revenue        = 0m,
                })
                .ToList();
        }

        return [];
    }

    /// <summary>
    /// Returns the top <see cref="MaxTopSessions"/> sessions by total engaged participants.
    /// Returns an empty list when engagement data is unavailable.
    /// </summary>
    private static List<SessionEngagementItem> BuildTopSessions(
        ExternalEngagementAnalyticsDto? engagement)
    {
        if (engagement?.Sessions is not { Count: > 0 })
            return [];

        return engagement.Sessions
            .OrderByDescending(s => s.TotalEngagedParticipants)
            .Take(MaxTopSessions)
            .Select(s => new SessionEngagementItem
            {
                SessionTitle             = s.SessionTitle,
                QuestionCount            = s.QuestionCount,
                ApprovedQuestionCount    = s.ApprovedQuestionCount,
                PollCount                = s.PollCount,
                TotalPollVotes           = s.TotalPollVotes,
                TotalEngagedParticipants = s.TotalEngagedParticipants,
            })
            .ToList();
    }

    /// <summary>
    /// Calculates average rating and extracts the most representative feedback highlights:
    /// • Positive: top-rated comments (≥ <see cref="PositiveRatingThreshold"/> ★), longest first for richness.
    /// • Critical: lowest-rated comments (≤ <see cref="CriticalRatingThreshold"/> ★), shortest first for clarity.
    ///
    /// Both lists are capped at <see cref="MaxFeedbackHighlights"/> entries each.
    /// Blank or whitespace-only comments are excluded to avoid LLM noise.
    /// </summary>
    private static (double? AverageRating, List<string> Positive, List<string> Critical)
        ProcessFeedback(IReadOnlyList<ExternalFeedbackDto> feedbacks)
    {
        if (feedbacks.Count == 0)
            return (null, [], []);

        var averageRating = Math.Round(feedbacks.Average(f => f.Rating), 2);

        var withComment = feedbacks
            .Where(f => !string.IsNullOrWhiteSpace(f.Comment))
            .ToList();

        var positive = withComment
            .Where(f => f.Rating >= PositiveRatingThreshold)
            .OrderByDescending(f => f.Rating)           // highest rated first
            .ThenByDescending(f => f.Comment.Length)    // richer comments preferred
            .Take(MaxFeedbackHighlights)
            .Select(f => f.Comment.Trim())
            .ToList();

        var critical = withComment
            .Where(f => f.Rating <= CriticalRatingThreshold)
            .OrderBy(f => f.Rating)                     // most critical first
            .ThenBy(f => f.Comment.Length)              // concise complaints preferred
            .Take(MaxFeedbackHighlights)
            .Select(f => f.Comment.Trim())
            .ToList();

        return (averageRating, positive, critical);
    }
}
