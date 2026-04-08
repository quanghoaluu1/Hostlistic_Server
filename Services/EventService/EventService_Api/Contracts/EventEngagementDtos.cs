using EventService_Domain.Enums;

namespace EventService_Api.Contracts;

public class EventEngagementStateDto
{
    public EventEngagementSessionDto? Session { get; set; }
    public List<EventEngagementQuestionDto> Questions { get; set; } = [];
    public EventEngagementPollDto? ActivePoll { get; set; }
    public List<EventEngagementPollDto> PollHistory { get; set; } = [];
    public bool CanModerate { get; set; }
    public bool CanAskQuestion { get; set; }
    public Guid? RequestedSessionId { get; set; }
    public bool CanSendChat { get; set; } = true;
    public DateTime? ChatBlockedUntil { get; set; }
    public DateTime? QaBlockedUntil { get; set; }
    public List<EventEngagementAttendeeDto> Attendees { get; set; } = [];
}

public class EventEngagementSessionDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public bool IsLiveWindow { get; set; }
}

public class EventEngagementQuestionDto
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string QuestionText { get; set; } = string.Empty;
    public QaStatus Status { get; set; }
    public int UpVotes { get; set; }
    public bool HasVotedByCurrentUser { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class EventEngagementPollDto
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public string Question { get; set; } = string.Empty;
    public PollType Type { get; set; }
    public bool IsActive { get; set; }
    public int? DurationInSecond { get; set; }
    public int TotalVotes { get; set; }
    public List<int> CurrentUserSelectionIds { get; set; } = [];
    public List<EventEngagementPollOptionDto> Options { get; set; } = [];
}

public class EventEngagementPollOptionDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int Votes { get; set; }
    public double Percentage { get; set; }
}

public class SubmitEventQuestionRequest
{
    public Guid? SessionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
}

public class UpdateEventQuestionStatusRequest
{
    public QaStatus Status { get; set; }
}

public class CreateEventPollRequest
{
    public Guid? SessionId { get; set; }
    public string Question { get; set; } = string.Empty;
    public List<string> Options { get; set; } = [];
    public int? DurationInSecond { get; set; }
    public PollType Type { get; set; } = PollType.Survey;
}

public class SubmitEventPollResponseRequest
{
    public List<int> SelectedOptionIds { get; set; } = [];
}

public class EventEngagementAttendeeDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool IsChatBlocked { get; set; }
    public bool IsQaBlocked { get; set; }
    public DateTime? ChatBlockedUntil { get; set; }
    public DateTime? QaBlockedUntil { get; set; }
}

public class UpdateEngagementRestrictionRequest
{
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public EngagementRestrictionScope Scope { get; set; }
    public bool IsBlocked { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? Reason { get; set; }
}

public class EventChatAccessDto
{
    public Guid SessionId { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool CanSendChat { get; set; }
    public DateTime? ChatBlockedUntil { get; set; }
}
