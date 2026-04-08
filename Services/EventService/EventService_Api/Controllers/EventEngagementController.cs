using Common;
using EventService_Api.Contracts;
using EventService_Api.Hubs;
using EventService_Domain.Entities;
using EventService_Domain.Enums;
using EventService_Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EventService_Api.Controllers;

[ApiController]
[Route("api/events/{eventId:guid}/engagement")]
[Authorize]
public class EventEngagementController : ControllerBase
{
    private readonly EventServiceDbContext _dbContext;
    private readonly IHubContext<EventEngagementHub> _hubContext;

    public EventEngagementController(
        EventServiceDbContext dbContext,
        IHubContext<EventEngagementHub> hubContext)
    {
        _dbContext = dbContext;
        _hubContext = hubContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetState(Guid eventId, [FromQuery] Guid? sessionId = null)
    {
        var userId = GetCurrentUserId();
        var access = await ResolveAccessAsync(eventId, userId);
        if (access is null)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(403, "You do not have access to this event."));
        }

        var state = await BuildStateAsync(eventId, userId, access.Role, sessionId);
        return Ok(ApiResponse<EventEngagementStateDto>.Success(200, "Engagement state retrieved successfully.", state));
    }

    [HttpGet("chat-access")]
    [AllowAnonymous]
    public async Task<IActionResult> GetChatAccess(Guid eventId, [FromQuery] Guid sessionId, [FromQuery] Guid userId)
    {
        var access = await ResolveAccessAsync(eventId, userId);
        if (access is null)
        {
            return NotFound(ApiResponse<object>.Fail(404, "Event not found."));
        }

        var session = await _dbContext.Sessions
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == sessionId && item.EventId == eventId);

        if (session is null)
        {
            return NotFound(ApiResponse<object>.Fail(404, "Session not found."));
        }

        var chatRestriction = await GetActiveRestrictionAsync(sessionId, userId, EngagementRestrictionScope.Chat);
        var payload = new EventChatAccessDto
        {
            SessionId = sessionId,
            UserId = userId,
            Role = access.Role.ToString(),
            CanSendChat = chatRestriction is null,
            ChatBlockedUntil = chatRestriction?.ExpiresAt
        };

        return Ok(ApiResponse<EventChatAccessDto>.Success(200, "Chat access retrieved successfully.", payload));
    }

    [HttpPost("questions")]
    public async Task<IActionResult> SubmitQuestion(Guid eventId, [FromBody] SubmitEventQuestionRequest request)
    {
        var userId = GetCurrentUserId();
        var access = await ResolveAccessAsync(eventId, userId);
        if (access is null)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(403, "You do not have access to this event."));
        }

        if (string.IsNullOrWhiteSpace(request.QuestionText))
        {
            return BadRequest(ApiResponse<object>.Fail(400, "Question text is required."));
        }

        var session = await ResolveSessionAsync(eventId, request.SessionId);
        if (session is null)
        {
            return NotFound(ApiResponse<object>.Fail(404, "No session is available for engagement right now."));
        }

        var qaRestriction = await GetActiveRestrictionAsync(session.Id, userId, EngagementRestrictionScope.Qa);
        if (qaRestriction is not null)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(403,
                qaRestriction.ExpiresAt.HasValue
                    ? $"Your Q&A access is blocked until {qaRestriction.ExpiresAt.Value:u}."
                    : "Your Q&A access is blocked for this session."));
        }

        var question = new QaQuestion
        {
            Id = Guid.NewGuid(),
            SessionId = session.Id,
            UserId = userId,
            QuestionText = request.QuestionText.Trim(),
            Status = CanModerate(access.Role) ? QaStatus.Approved : QaStatus.Pending,
            UpVotes = 0,
            AskedAt = session.StartTime.HasValue ? DateTime.UtcNow - session.StartTime.Value : TimeSpan.Zero,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.QaQuestions.Add(question);
        await _dbContext.SaveChangesAsync();
        await BroadcastEngagementChangedAsync(eventId, "question-created", question.Id);

        return Ok(ApiResponse<object>.Success(200, "Question submitted successfully.", new { questionId = question.Id }));
    }

    [HttpPost("questions/{questionId:guid}/vote")]
    public async Task<IActionResult> ToggleQuestionVote(Guid eventId, Guid questionId)
    {
        var userId = GetCurrentUserId();
        var access = await ResolveAccessAsync(eventId, userId);
        if (access is null)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(403, "You do not have access to this event."));
        }

        var question = await _dbContext.QaQuestions
            .Include(q => q.Session)
            .FirstOrDefaultAsync(q => q.Id == questionId && q.Session.EventId == eventId);

        if (question is null)
        {
            return NotFound(ApiResponse<object>.Fail(404, "Question not found."));
        }

        var qaRestriction = await GetActiveRestrictionAsync(question.SessionId, userId, EngagementRestrictionScope.Qa);
        if (qaRestriction is not null)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(403,
                qaRestriction.ExpiresAt.HasValue
                    ? $"Your Q&A access is blocked until {qaRestriction.ExpiresAt.Value:u}."
                    : "Your Q&A access is blocked for this session."));
        }

        var existingVote = await _dbContext.QaVotes
            .FirstOrDefaultAsync(v => v.QaQuestionId == questionId && v.UserId == userId);

        var hasVoted = false;
        if (existingVote is null)
        {
            _dbContext.QaVotes.Add(new QaVote
            {
                QaQuestionId = questionId,
                UserId = userId,
                VotedAt = DateTime.UtcNow
            });
            question.UpVotes += 1;
            hasVoted = true;
        }
        else
        {
            _dbContext.QaVotes.Remove(existingVote);
            question.UpVotes = Math.Max(0, question.UpVotes - 1);
        }

        question.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        await BroadcastEngagementChangedAsync(eventId, "question-voted", question.Id);

        return Ok(ApiResponse<object>.Success(200, "Question vote updated successfully.", new
        {
            questionId = question.Id,
            hasVoted,
            upVotes = question.UpVotes
        }));
    }

    [HttpPatch("questions/{questionId:guid}/status")]
    public async Task<IActionResult> UpdateQuestionStatus(Guid eventId, Guid questionId, [FromBody] UpdateEventQuestionStatusRequest request)
    {
        var userId = GetCurrentUserId();
        var access = await ResolveAccessAsync(eventId, userId);
        if (access is null || !CanModerate(access.Role))
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(403, "You are not allowed to moderate questions."));
        }

        var question = await _dbContext.QaQuestions
            .Include(q => q.Session)
            .FirstOrDefaultAsync(q => q.Id == questionId && q.Session.EventId == eventId);

        if (question is null)
        {
            return NotFound(ApiResponse<object>.Fail(404, "Question not found."));
        }

        question.Status = request.Status;
        question.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        await BroadcastEngagementChangedAsync(eventId, "question-status", question.Id);

        return Ok(ApiResponse<object>.Success(200, "Question status updated successfully.", new
        {
            questionId = question.Id,
            status = question.Status
        }));
    }

    [HttpDelete("questions/{questionId:guid}")]
    public async Task<IActionResult> DeleteQuestion(Guid eventId, Guid questionId)
    {
        var userId = GetCurrentUserId();
        var access = await ResolveAccessAsync(eventId, userId);
        if (access is null || !CanModerate(access.Role))
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(403, "You are not allowed to delete questions."));
        }

        var question = await _dbContext.QaQuestions
            .Include(q => q.Session)
            .FirstOrDefaultAsync(q => q.Id == questionId && q.Session.EventId == eventId);

        if (question is null)
        {
            return NotFound(ApiResponse<object>.Fail(404, "Question not found."));
        }

        _dbContext.QaQuestions.Remove(question);
        await _dbContext.SaveChangesAsync();
        await BroadcastEngagementChangedAsync(eventId, "question-deleted", questionId);

        return Ok(ApiResponse<object>.Success(200, "Question deleted successfully.", new
        {
            questionId
        }));
    }

    [HttpPost("polls")]
    public async Task<IActionResult> CreatePoll(Guid eventId, [FromBody] CreateEventPollRequest request)
    {
        var userId = GetCurrentUserId();
        var access = await ResolveAccessAsync(eventId, userId);
        if (access is null || !CanModerate(access.Role))
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(403, "You are not allowed to create polls."));
        }

        if (string.IsNullOrWhiteSpace(request.Question))
        {
            return BadRequest(ApiResponse<object>.Fail(400, "Poll question is required."));
        }

        var cleanOptions = request.Options
            .Where(option => !string.IsNullOrWhiteSpace(option))
            .Select(option => option.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (cleanOptions.Count < 2)
        {
            return BadRequest(ApiResponse<object>.Fail(400, "At least two poll options are required."));
        }

        var session = await ResolveSessionAsync(eventId, request.SessionId);
        if (session is null)
        {
            return NotFound(ApiResponse<object>.Fail(404, "No session is available for engagement right now."));
        }

        var activePolls = await _dbContext.Polls
            .Where(p => p.SessionId == session.Id && p.IsActive)
            .ToListAsync();

        foreach (var existingPoll in activePolls)
        {
            existingPoll.IsActive = false;
            existingPoll.UpdatedAt = DateTime.UtcNow;
        }

        var poll = new Poll
        {
            Id = Guid.NewGuid(),
            SessionId = session.Id,
            Question = request.Question.Trim(),
            Options = cleanOptions.Select((text, index) => new PollOption
            {
                Id = index,
                Order = index,
                Text = text
            }).ToList(),
            CorrectAnswers = [],
            Type = request.Type,
            IsPrivate = false,
            DurationInSecond = request.DurationInSecond,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Polls.Add(poll);
        await _dbContext.SaveChangesAsync();
        await BroadcastEngagementChangedAsync(eventId, "poll-created", poll.Id);

        return Ok(ApiResponse<object>.Success(200, "Poll created successfully.", new { pollId = poll.Id }));
    }

    [HttpPost("polls/{pollId:guid}/responses")]
    public async Task<IActionResult> SubmitPollResponse(Guid eventId, Guid pollId, [FromBody] SubmitEventPollResponseRequest request)
    {
        var userId = GetCurrentUserId();
        var access = await ResolveAccessAsync(eventId, userId);
        if (access is null)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(403, "You do not have access to this event."));
        }

        var poll = await _dbContext.Polls
            .Include(p => p.Session)
            .FirstOrDefaultAsync(p => p.Id == pollId && p.Session.EventId == eventId);

        if (poll is null)
        {
            return NotFound(ApiResponse<object>.Fail(404, "Poll not found."));
        }

        if (!poll.IsActive)
        {
            return BadRequest(ApiResponse<object>.Fail(400, "This poll is already closed."));
        }

        var selectedOptionIds = request.SelectedOptionIds.Distinct().ToArray();
        if (selectedOptionIds.Length == 0)
        {
            return BadRequest(ApiResponse<object>.Fail(400, "Select at least one poll option."));
        }

        var validOptionIds = poll.Options.Select(option => option.Id).ToHashSet();
        if (selectedOptionIds.Any(optionId => !validOptionIds.Contains(optionId)))
        {
            return BadRequest(ApiResponse<object>.Fail(400, "One or more selected options are invalid."));
        }

        if (poll.Type == PollType.Survey && selectedOptionIds.Length > 1)
        {
            return BadRequest(ApiResponse<object>.Fail(400, "Survey polls only allow one selected option."));
        }

        var existingResponse = await _dbContext.PollResponses
            .FirstOrDefaultAsync(response => response.PollId == pollId && response.UserId == userId);

        if (existingResponse is null)
        {
            _dbContext.PollResponses.Add(new PollResponse
            {
                Id = Guid.NewGuid(),
                PollId = pollId,
                UserId = userId,
                SelectedOptionId = selectedOptionIds,
                RespondedAt = DateTime.UtcNow
            });
        }
        else
        {
            existingResponse.SelectedOptionId = selectedOptionIds;
            existingResponse.RespondedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();
        await BroadcastEngagementChangedAsync(eventId, "poll-response", poll.Id);

        return Ok(ApiResponse<object>.Success(200, "Poll response submitted successfully.", new
        {
            pollId = poll.Id,
            selectedOptionIds
        }));
    }

    [HttpPost("polls/{pollId:guid}/close")]
    public async Task<IActionResult> ClosePoll(Guid eventId, Guid pollId)
    {
        var userId = GetCurrentUserId();
        var access = await ResolveAccessAsync(eventId, userId);
        if (access is null || !CanModerate(access.Role))
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(403, "You are not allowed to close polls."));
        }

        var poll = await _dbContext.Polls
            .Include(p => p.Session)
            .FirstOrDefaultAsync(p => p.Id == pollId && p.Session.EventId == eventId);

        if (poll is null)
        {
            return NotFound(ApiResponse<object>.Fail(404, "Poll not found."));
        }

        poll.IsActive = false;
        poll.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        await BroadcastEngagementChangedAsync(eventId, "poll-closed", poll.Id);

        return Ok(ApiResponse<object>.Success(200, "Poll closed successfully.", new { pollId = poll.Id }));
    }

    [HttpPost("restrictions")]
    public async Task<IActionResult> UpdateRestriction(Guid eventId, [FromBody] UpdateEngagementRestrictionRequest request)
    {
        var userId = GetCurrentUserId();
        var access = await ResolveAccessAsync(eventId, userId);
        if (access is null || !CanModerate(access.Role))
        {
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(403, "You are not allowed to manage attendee restrictions."));
        }

        var session = await _dbContext.Sessions
            .FirstOrDefaultAsync(item => item.Id == request.SessionId && item.EventId == eventId);

        if (session is null)
        {
            return NotFound(ApiResponse<object>.Fail(404, "Session not found."));
        }

        if (request.UserId == userId)
        {
            return BadRequest(ApiResponse<object>.Fail(400, "You cannot apply a restriction to yourself."));
        }

        var targetAccess = await ResolveAccessAsync(eventId, request.UserId);
        if (targetAccess is null)
        {
            return NotFound(ApiResponse<object>.Fail(404, "Attendee not found in this event."));
        }

        if (CanModerate(targetAccess.Role))
        {
            return BadRequest(ApiResponse<object>.Fail(400, "You can only restrict attendees."));
        }

        if (request.IsBlocked && request.ExpiresAt.HasValue && request.ExpiresAt.Value <= DateTime.UtcNow)
        {
            return BadRequest(ApiResponse<object>.Fail(400, "Restriction expiry must be in the future."));
        }

        var existingRestrictions = await _dbContext.SessionEngagementRestrictions
            .Where(item => item.EventId == eventId
                && item.SessionId == request.SessionId
                && item.UserId == request.UserId
                && item.Scope == request.Scope
                && item.IsActive)
            .ToListAsync();

        foreach (var existing in existingRestrictions)
        {
            existing.IsActive = false;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        if (request.IsBlocked)
        {
            _dbContext.SessionEngagementRestrictions.Add(new SessionEngagementRestriction
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                SessionId = request.SessionId,
                UserId = request.UserId,
                Scope = request.Scope,
                ExpiresAt = request.ExpiresAt,
                IsActive = true,
                Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim(),
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await _dbContext.SaveChangesAsync();
        await BroadcastEngagementChangedAsync(eventId, "restriction-updated", request.UserId);

        return Ok(ApiResponse<object>.Success(200, "Engagement restriction updated successfully.", new
        {
            sessionId = request.SessionId,
            userId = request.UserId,
            scope = request.Scope,
            isBlocked = request.IsBlocked,
            expiresAt = request.ExpiresAt
        }));
    }

    private async Task<EventEngagementStateDto> BuildStateAsync(Guid eventId, Guid userId, EventRole role, Guid? sessionId)
    {
        var session = await ResolveSessionAsync(eventId, sessionId);
        if (session is null)
        {
            return new EventEngagementStateDto
            {
                CanModerate = CanModerate(role),
                CanAskQuestion = true,
                CanSendChat = true,
                RequestedSessionId = sessionId
            };
        }

        var membershipNames = await _dbContext.EventTeamMembers
            .Where(member => member.EventId == eventId)
            .ToDictionaryAsync(member => member.UserId, member => member.UserFullName);

        var membershipEmails = await _dbContext.EventTeamMembers
            .Where(member => member.EventId == eventId)
            .ToDictionaryAsync(member => member.UserId, member => member.UserEmail);

        var restrictions = await GetActiveRestrictionsAsync(session.Id);
        var restrictionLookup = restrictions
            .GroupBy(item => new { item.UserId, item.Scope })
            .ToDictionary(group => (group.Key.UserId, group.Key.Scope), group => group
                .OrderByDescending(item => item.ExpiresAt.HasValue)
                .ThenBy(item => item.ExpiresAt)
                .First());

        var currentQaRestriction = restrictionLookup.GetValueOrDefault((userId, EngagementRestrictionScope.Qa));
        var currentChatRestriction = restrictionLookup.GetValueOrDefault((userId, EngagementRestrictionScope.Chat));

        var questionsQuery = _dbContext.QaQuestions
            .Where(question => question.SessionId == session.Id);

        // Attendee flow: submitted questions wait for moderation.
        // Non-moderators only see approved questions plus their own pending items.
        if (!CanModerate(role))
        {
            questionsQuery = questionsQuery.Where(question =>
                question.Status == QaStatus.Approved
                || (question.UserId == userId && question.Status == QaStatus.Pending));
        }

        var questions = await questionsQuery
            .OrderBy(question => question.Status == QaStatus.Approved ? 0 : question.Status == QaStatus.Pending ? 1 : 2)
            .ThenByDescending(question => question.UpVotes)
            .ThenBy(question => question.CreatedAt)
            .ToListAsync();

        var votedQuestionIds = await _dbContext.QaVotes
            .Where(vote => vote.UserId == userId && questions.Select(question => question.Id).Contains(vote.QaQuestionId))
            .Select(vote => vote.QaQuestionId)
            .ToListAsync();

        var activePoll = await _dbContext.Polls
            .Include(poll => poll.PollResponses)
            .Where(poll => poll.SessionId == session.Id && poll.IsActive)
            .OrderByDescending(poll => poll.CreatedAt)
            .FirstOrDefaultAsync();

        var closedPolls = await _dbContext.Polls
            .Include(poll => poll.PollResponses)
            .Where(poll => poll.SessionId == session.Id && !poll.IsActive)
            .OrderByDescending(poll => poll.UpdatedAt)
            .Take(8)
            .ToListAsync();

        return new EventEngagementStateDto
        {
            Session = new EventEngagementSessionDto
            {
                Id = session.Id,
                Title = session.Title,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                IsLiveWindow = IsLiveWindow(session)
            },
            Questions = questions.Select(question => new EventEngagementQuestionDto
            {
                Id = question.Id,
                SessionId = question.SessionId,
                UserId = question.UserId,
                AuthorName = ResolveAuthorName(membershipNames, question.UserId),
                QuestionText = question.QuestionText,
                Status = question.Status,
                UpVotes = question.UpVotes,
                HasVotedByCurrentUser = votedQuestionIds.Contains(question.Id),
                CreatedAt = question.CreatedAt
            }).ToList(),
            ActivePoll = activePoll is null ? null : MapPoll(activePoll, userId),
            PollHistory = closedPolls.Select(poll => MapPoll(poll, userId)).ToList(),
            CanModerate = CanModerate(role),
            CanAskQuestion = currentQaRestriction is null,
            CanSendChat = currentChatRestriction is null,
            QaBlockedUntil = currentQaRestriction?.ExpiresAt,
            ChatBlockedUntil = currentChatRestriction?.ExpiresAt,
            Attendees = CanModerate(role)
                ? await BuildAttendeeStateAsync(eventId, session.Id, membershipNames, membershipEmails, restrictionLookup)
                : [],
            RequestedSessionId = sessionId ?? session.Id
        };
    }

    private async Task<ResolvedEventAccess?> ResolveAccessAsync(Guid eventId, Guid userId)
    {
        var eventEntity = await _dbContext.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == eventId);

        if (eventEntity is null)
        {
            return null;
        }

        if (eventEntity.OrganizerId == userId)
        {
            return new ResolvedEventAccess(eventEntity, EventRole.Organizer);
        }

        var membership = await _dbContext.EventTeamMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(member => member.EventId == eventId
                && member.UserId == userId
                && member.Status == EventMemberStatus.Active);

        if (membership is not null)
        {
            return new ResolvedEventAccess(eventEntity, membership.Role);
        }

        return new ResolvedEventAccess(eventEntity, EventRole.Attendee);
    }

    private async Task<Session?> ResolveSessionAsync(Guid eventId, Guid? sessionId)
    {
        if (sessionId.HasValue)
        {
            return await _dbContext.Sessions
                .FirstOrDefaultAsync(session => session.Id == sessionId && session.EventId == eventId);
        }

        var now = DateTime.UtcNow;
        var sessions = await _dbContext.Sessions
            .Where(session => session.EventId == eventId)
            .OrderBy(session => session.StartTime ?? DateTime.MaxValue)
            .ThenBy(session => session.SortOrder)
            .ToListAsync();

        return sessions
            .OrderBy(session => session.StartTime.HasValue && session.EndTime.HasValue && session.StartTime <= now && session.EndTime >= now ? 0 : session.StartTime.HasValue && session.StartTime > now ? 1 : 2)
            .FirstOrDefault();
    }

    private static EventEngagementPollDto MapPoll(Poll poll, Guid userId)
    {
        var voteMap = poll.Options.ToDictionary(option => option.Id, _ => 0);

        foreach (var response in poll.PollResponses)
        {
            foreach (var selectedOptionId in response.SelectedOptionId)
            {
                if (voteMap.ContainsKey(selectedOptionId))
                {
                    voteMap[selectedOptionId] += 1;
                }
            }
        }

        var totalVotes = voteMap.Values.Sum();
        var currentUserResponse = poll.PollResponses.FirstOrDefault(response => response.UserId == userId);

        return new EventEngagementPollDto
        {
            Id = poll.Id,
            SessionId = poll.SessionId,
            Question = poll.Question,
            Type = poll.Type,
            IsActive = poll.IsActive,
            DurationInSecond = poll.DurationInSecond,
            TotalVotes = totalVotes,
            CurrentUserSelectionIds = currentUserResponse?.SelectedOptionId.ToList() ?? [],
            Options = poll.Options
                .OrderBy(option => option.Order)
                .Select(option =>
                {
                    var votes = voteMap.GetValueOrDefault(option.Id, 0);
                    return new EventEngagementPollOptionDto
                    {
                        Id = option.Id,
                        Text = option.Text,
                        ImageUrl = option.ImageUrl,
                        Votes = votes,
                        Percentage = totalVotes == 0 ? 0 : Math.Round((double)votes * 100 / totalVotes, 1)
                    };
                })
                .ToList()
        };
    }

    private static bool CanModerate(EventRole role)
    {
        return role is EventRole.Organizer or EventRole.CoOrganizer or EventRole.Staff;
    }

    private async Task<List<SessionEngagementRestriction>> GetActiveRestrictionsAsync(Guid sessionId)
    {
        var now = DateTime.UtcNow;
        return await _dbContext.SessionEngagementRestrictions
            .Where(item => item.SessionId == sessionId
                && item.IsActive
                && (!item.ExpiresAt.HasValue || item.ExpiresAt > now))
            .ToListAsync();
    }

    private async Task<SessionEngagementRestriction?> GetActiveRestrictionAsync(Guid sessionId, Guid userId, EngagementRestrictionScope scope)
    {
        var now = DateTime.UtcNow;
        return await _dbContext.SessionEngagementRestrictions
            .Where(item => item.SessionId == sessionId
                && item.UserId == userId
                && item.Scope == scope
                && item.IsActive
                && (!item.ExpiresAt.HasValue || item.ExpiresAt > now))
            .OrderBy(item => item.ExpiresAt)
            .FirstOrDefaultAsync();
    }

    private async Task<List<EventEngagementAttendeeDto>> BuildAttendeeStateAsync(
        Guid eventId,
        Guid sessionId,
        IReadOnlyDictionary<Guid, string?> membershipNames,
        IReadOnlyDictionary<Guid, string?> membershipEmails,
        IReadOnlyDictionary<(Guid UserId, EngagementRestrictionScope Scope), SessionEngagementRestriction> restrictionLookup)
    {
        var attendeeUserIds = await _dbContext.SessionBookings
            .Where(booking => booking.SessionId == sessionId && booking.Status == BookingStatus.Confirmed)
            .Select(booking => booking.UserId)
            .Distinct()
            .ToListAsync();

        var questionAuthors = await _dbContext.QaQuestions
            .Where(question => question.SessionId == sessionId)
            .Select(question => question.UserId)
            .Distinct()
            .ToListAsync();

        var attendeeMembers = await _dbContext.EventTeamMembers
            .Where(member => member.EventId == eventId
                && member.Status == EventMemberStatus.Active
                && member.Role == EventRole.Attendee)
            .Select(member => member.UserId)
            .Distinct()
            .ToListAsync();

        var users = attendeeUserIds
            .Concat(questionAuthors)
            .Concat(attendeeMembers)
            .Distinct()
            .ToList();

        var moderatorIds = await _dbContext.EventTeamMembers
            .Where(member => member.EventId == eventId
                && member.Status == EventMemberStatus.Active
                && (member.Role == EventRole.CoOrganizer || member.Role == EventRole.Staff))
            .Select(member => member.UserId)
            .ToListAsync();

        var organizerId = await _dbContext.Events
            .Where(item => item.Id == eventId)
            .Select(item => item.OrganizerId)
            .FirstOrDefaultAsync();

        return users
            .Where(user => user != organizerId && !moderatorIds.Contains(user))
            .Select(attendeeUserId =>
            {
                var chatRestriction = restrictionLookup.GetValueOrDefault((attendeeUserId, EngagementRestrictionScope.Chat));
                var qaRestriction = restrictionLookup.GetValueOrDefault((attendeeUserId, EngagementRestrictionScope.Qa));

                return new EventEngagementAttendeeDto
                {
                    UserId = attendeeUserId,
                    Name = ResolveAuthorName(membershipNames, attendeeUserId),
                    Email = membershipEmails.GetValueOrDefault(attendeeUserId),
                    IsChatBlocked = chatRestriction is not null,
                    IsQaBlocked = qaRestriction is not null,
                    ChatBlockedUntil = chatRestriction?.ExpiresAt,
                    QaBlockedUntil = qaRestriction?.ExpiresAt
                };
            })
            .OrderBy(item => item.Name)
            .ToList();
    }

    private static bool IsLiveWindow(Session session)
    {
        var now = DateTime.UtcNow;
        return session.StartTime.HasValue && session.EndTime.HasValue && session.StartTime <= now && session.EndTime >= now;
    }

    private static string ResolveAuthorName(IReadOnlyDictionary<Guid, string?> membershipNames, Guid userId)
    {
        if (membershipNames.TryGetValue(userId, out var displayName) && !string.IsNullOrWhiteSpace(displayName))
        {
            return displayName;
        }

        return $"User {userId.ToString()[..8]}";
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException("User ID not found in token"));
    }

    private Task BroadcastEngagementChangedAsync(Guid eventId, string scope, Guid entityId)
    {
        return _hubContext.Clients
            .Group(EventEngagementHub.BuildEventGroup(eventId.ToString()))
            .SendAsync("EngagementChanged", new
            {
                eventId,
                scope,
                entityId,
                occurredAt = DateTime.UtcNow
            });
    }

    private sealed record ResolvedEventAccess(Event Event, EventRole Role);
}
