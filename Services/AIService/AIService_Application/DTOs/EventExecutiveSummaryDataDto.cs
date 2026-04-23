namespace AIService_Application.DTOs;

/// <summary>
/// The fully-aggregated, LLM-ready context object for a post-event executive summary.
/// All heavyweight data (raw orders, individual check-in records, etc.) has already been
/// processed and minimized so only the semantically rich fields reach the prompt.
/// </summary>
public class EventExecutiveSummaryDataDto
{
    // ── Event Identity ────────────────────────────────────────────────────────

    public Guid EventId { get; set; }
    public string EventName { get; set; } = string.Empty;

    // ── Ticket & Attendance ───────────────────────────────────────────────────

    /// <summary>Total confirmed tickets sold across all ticket types.</summary>
    public int TotalTicketsSold { get; set; }

    /// <summary>Number of unique tickets that completed an event-level check-in.</summary>
    public int TotalCheckedIn { get; set; }

    /// <summary>CheckedIn / TicketsSold × 100, rounded to 1 decimal place. 0 when no tickets sold.</summary>
    public double CheckInRate { get; set; }

    // ── Revenue ───────────────────────────────────────────────────────────────

    /// <summary>Gross revenue from all confirmed orders (VND / the event's currency).</summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>Ticket-type breakdown: name, sold count, check-in count, and revenue contribution.</summary>
    public List<TicketTypeSummaryItem> ByTicketType { get; set; } = [];

    // ── Engagement (Q&A / Polls) ──────────────────────────────────────────────

    /// <summary>Total Q&amp;A questions submitted across all sessions.</summary>
    public int TotalQuestions { get; set; }

    /// <summary>Number of questions that were approved by moderators.</summary>
    public int ApprovedQuestions { get; set; }

    /// <summary>Total poll votes cast across all sessions.</summary>
    public int TotalPollVotes { get; set; }

    /// <summary>
    /// Up to 5 sessions sorted by total engaged participants (desc).
    /// Gives the LLM a sense of which sessions resonated most.
    /// </summary>
    public List<SessionEngagementItem> TopEngagedSessions { get; set; } = [];

    // ── Attendee Feedback ─────────────────────────────────────────────────────

    /// <summary>Average star rating across all feedback submissions (1–5 scale). Null when no feedback.</summary>
    public double? AverageRating { get; set; }

    /// <summary>Total number of feedback submissions.</summary>
    public int TotalFeedbackCount { get; set; }

    /// <summary>Up to 5 comments from attendees who rated 4–5 stars (positive signal).</summary>
    public List<string> HighlightPositiveFeedbacks { get; set; } = [];

    /// <summary>Up to 5 comments from attendees who rated 1–3 stars (improvement signal).</summary>
    public List<string> HighlightCriticalFeedbacks { get; set; } = [];

    // ── Data Quality Signals ──────────────────────────────────────────────────

    /// <summary>True when at least one data source returned an empty/null result (partial fetch).</summary>
    public bool HasPartialData { get; set; }

    /// <summary>Human-readable list of data sources that returned no data, for transparency in the LLM prompt.</summary>
    public List<string> MissingDataSources { get; set; } = [];
}

// ── Supporting Value Objects ──────────────────────────────────────────────────

public class TicketTypeSummaryItem
{
    public string TicketTypeName { get; set; } = string.Empty;
    public int TicketCount { get; set; }
    public int CheckedInCount { get; set; }
    public decimal Revenue { get; set; }
}

public class SessionEngagementItem
{
    public string SessionTitle { get; set; } = string.Empty;
    public int QuestionCount { get; set; }
    public int ApprovedQuestionCount { get; set; }
    public int PollCount { get; set; }
    public int TotalPollVotes { get; set; }
    public int TotalEngagedParticipants { get; set; }
}
