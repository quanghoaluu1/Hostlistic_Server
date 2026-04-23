namespace AIService_Application.DTOs.External;

/// <summary>
/// Mirrors EventService EventEngagementAnalyticsDto.
/// Source: GET /api/events/{eventId}/engagement/analytics
/// </summary>
public class ExternalEngagementAnalyticsDto
{
    public int TotalQuestions { get; set; }
    public int ApprovedQuestions { get; set; }
    public int UniqueQuestionParticipants { get; set; }
    public int TotalPolls { get; set; }
    public int ActivePolls { get; set; }
    public int PollResponseCount { get; set; }
    public int UniquePollParticipants { get; set; }
    public int TotalPollVotes { get; set; }
    public int TotalEngagedParticipants { get; set; }
    public List<ExternalSessionEngagementAnalyticsDto> Sessions { get; set; } = [];
}

public class ExternalSessionEngagementAnalyticsDto
{
    public Guid SessionId { get; set; }
    public string SessionTitle { get; set; } = string.Empty;
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int QuestionCount { get; set; }
    public int ApprovedQuestionCount { get; set; }
    public int UniqueQuestionParticipants { get; set; }
    public int PollCount { get; set; }
    public int ActivePollCount { get; set; }
    public int PollResponseCount { get; set; }
    public int UniquePollParticipants { get; set; }
    public int TotalPollVotes { get; set; }
    public int TotalEngagedParticipants { get; set; }
}
