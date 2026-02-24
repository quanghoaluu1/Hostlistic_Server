using Common;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using EventService_Domain.Entities;
using EventService_Domain.Enums;
using EventService_Domain.Interfaces;
using Mapster;

namespace EventService_Application.Services
{
    public class QaQuestionService : IQaQuestionService
    {
        private readonly IQaQuestionRepository _qaQuestionRepository;
        private readonly ISessionRepository _sessionRepository;
        private readonly IEventRepository _eventRepository;
        public QaQuestionService(IQaQuestionRepository qaQuestionRepository, ISessionRepository sessionRepository, IEventRepository eventRepository)
        {
            _qaQuestionRepository = qaQuestionRepository;
            _sessionRepository = sessionRepository;
            _eventRepository = eventRepository;
        }

        public async Task<ApiResponse<QaQuestionDto>> AddQaQuestionAsync(CreateQaQuestionDto request)
        {
            var existingSession = await _sessionRepository.GetSessionByIdAsync(request.SessionId);
            if (existingSession == null)
            {
                return ApiResponse<QaQuestionDto>.Fail(404, "Session not found");
            }
            var existingEvent = await _eventRepository.GetEventByIdAsync(existingSession.EventId);
            if (existingEvent == null)
            {
                return ApiResponse<QaQuestionDto>.Fail(404, "Event not found");
            }
            var validUser = existingEvent.EventTeamMembers.FirstOrDefault(a => a.UserId == request.UserId);
            if (validUser == null || validUser.Role != EventRole.Attendee)
            {
                return ApiResponse<QaQuestionDto>.Fail(403, "User is not a atendee of the event");

            }
            var now = DateTime.UtcNow;

            if (now < existingSession.StartTime || now > existingSession.EndTime)
                return ApiResponse<QaQuestionDto>.Fail(400, "Session is not active");
            var askedAt = DateTime.UtcNow - existingSession.StartTime;
            var sessionDuration = existingSession.EndTime
                      - existingSession.StartTime;
            if (askedAt < TimeSpan.Zero || askedAt > sessionDuration)
            {
                return ApiResponse<QaQuestionDto>.Fail(400, "Question must be asked during the session time");
            }
            var newQaQuestion = new QaQuestion
            {
                Id = Guid.NewGuid(),
                SessionId = request.SessionId,
                UserId = request.UserId,
                QuestionText = request.QuestionText,
                Status = QaStatus.Pending,
                UpVotes = 0,
                AskedAt = (TimeSpan)(askedAt)
            };


            await _qaQuestionRepository.AddQaQuestionAsync(newQaQuestion);
            var qaQuestionDto = newQaQuestion.Adapt<QaQuestionDto>();
            return ApiResponse<QaQuestionDto>.Success(200, "Created QaQuestion Successfully", qaQuestionDto);
        }

        public async Task<ApiResponse<QaQuestionDto>> UpdateQaQuestionStatusAsync(Guid qaQuestionId, QaStatus newStatus)
        {
            var existingQaQuestion = await _qaQuestionRepository.GetQaQuestionByIdAsync(qaQuestionId);
            if (existingQaQuestion == null)
            {
                return ApiResponse<QaQuestionDto>.Fail(404, "QaQuestion not found");
            }
            var existingSession = await _sessionRepository.GetSessionByIdAsync(existingQaQuestion.SessionId);
            if (existingSession == null)
            {
                return ApiResponse<QaQuestionDto>.Fail(404, "Session not found");
            }
            var existingEvent = await _eventRepository.GetEventByIdAsync(existingSession.EventId);
            if (existingEvent == null)
            {
                return ApiResponse<QaQuestionDto>.Fail(404, "Event not found");
            }
            var validUser = existingEvent.EventTeamMembers.FirstOrDefault(a => a.UserId == existingQaQuestion.UserId);
            if (validUser == null || validUser.Role != EventRole.Staff)
            {
                return ApiResponse<QaQuestionDto>.Fail(403, "User is not a allowed to update Status");
            }
            existingQaQuestion.Status = newStatus;
            await _qaQuestionRepository.UpdateQaQuestionAsync(existingQaQuestion);
            var qaQuestionDto = existingQaQuestion.Adapt<QaQuestionDto>();
            return ApiResponse<QaQuestionDto>.Success(200, "Updated QaQuestion status successfully", qaQuestionDto);
        }

        public async Task<ApiResponse<bool>> DeleteQaQuestionAsync(Guid qaQuestionId)
        {
            var existingQaQuestion = await _qaQuestionRepository.GetQaQuestionByIdAsync(qaQuestionId);
            if (existingQaQuestion == null)
            {
                return ApiResponse<bool>.Fail(404, "QaQuestion not found");
            }
            await _qaQuestionRepository.DeleteQaQuestionAsync(existingQaQuestion);
            return ApiResponse<bool>.Success(200, "Deleted QaQuestion successfully", true);
        }

        public async Task<ApiResponse<List<QaQuestionDto>>> GetQaQuestionsBySessionIdAsync(Guid sessionId)
        {
            var existingSession = await _sessionRepository.GetSessionByIdAsync(sessionId);
            if (existingSession == null)
            {
                return ApiResponse<List<QaQuestionDto>>.Fail(404, "Session not found");
            }
            var qaQuestions = await _qaQuestionRepository.GetQaQuestionsBySessionIdAsync(sessionId);
            var qaQuestionDtos = qaQuestions.Adapt<List<QaQuestionDto>>();
            return ApiResponse<List<QaQuestionDto>>.Success(200, "Retrieved QaQuestions successfully", qaQuestionDtos);
        }
        public async Task<ApiResponse<List<QaQuestionDto>>> GetQaQuestionsByUserIdAsync(Guid userId)
        {
            var qaQuestions = await _qaQuestionRepository.GetQaQuestionsByUserIdAsync(userId);
            var qaQuestionDtos = qaQuestions.Adapt<List<QaQuestionDto>>();
            return ApiResponse<List<QaQuestionDto>>.Success(200, "Retrieved QaQuestions successfully", qaQuestionDtos);
        }

        public async Task<ApiResponse<QaQuestionDto>> QaQuestionVote(QaVoteDto request)
        {

            var existingQaQuestion = await _qaQuestionRepository.GetQaQuestionByIdAsync(request.QaQuestionId);
            if (existingQaQuestion == null)
            {
                return ApiResponse<QaQuestionDto>.Fail(404, "QaQuestion not found");
            }
            var existingSession = await _sessionRepository.GetSessionByIdAsync(existingQaQuestion.SessionId);
            if (existingSession == null)
            {
                return ApiResponse<QaQuestionDto>.Fail(404, "Session not found");
            }
            var existingEvent = await _eventRepository.GetEventByIdAsync(existingSession.EventId);
            if (existingEvent == null)
            {
                return ApiResponse<QaQuestionDto>.Fail(404, "Event not found");
            }
            var validUser = existingEvent.EventTeamMembers.FirstOrDefault(a => a.UserId == request.UserId);
            if (validUser == null || validUser.Role != EventRole.Attendee)
            {
                return ApiResponse<QaQuestionDto>.Fail(403, "User is not a allowed to vote Event");
            }
            var voteAt = DateTime.UtcNow;
            if (voteAt < existingSession.StartTime || voteAt > existingSession.EndTime)
                return ApiResponse<QaQuestionDto>.Fail(400, "Session is not active");
            var newVote = new QaVote
            {
                UserId = request.UserId,
                QaQuestionId = request.QaQuestionId,
                VotedAt = voteAt
            };
            var result = await _qaQuestionRepository.QaQuestionVote(newVote);
            if (!result)
            {
                return ApiResponse<QaQuestionDto>.Fail(400, "User has already voted for this question");
            }
            existingQaQuestion.UpVotes += 1;
            await _qaQuestionRepository.UpdateQaQuestionAsync(existingQaQuestion);
            var qaQuestionDto = existingQaQuestion.Adapt<QaQuestionDto>();
            return ApiResponse<QaQuestionDto>.Success(200, "Voted successfully", qaQuestionDto);
        }

        public async Task<ApiResponse<int>> GetQaQuestionVotes(Guid qaQuestionId)
        {
            var existingQaQuestion = await _qaQuestionRepository.GetQaQuestionByIdAsync(qaQuestionId);
            if (existingQaQuestion == null)
            {
                return ApiResponse<int>.Fail(404, "QaQuestion not found");
            }
            var votes = await _qaQuestionRepository.GetQaQuestionVotes(qaQuestionId);
            return ApiResponse<int>.Success(200, "Get Quétion votes successfully", votes);
        }

        public async Task<ApiResponse<QaQuestion>> GetQaQuestionByIdAsync(Guid qaQuestionId)
        {
            var existingQaQuestion = await _qaQuestionRepository.GetQaQuestionByIdAsync(qaQuestionId);
            if (existingQaQuestion == null)
            {
                return ApiResponse<QaQuestion>.Fail(404, "QaQuestion not found");
            }
            return ApiResponse<QaQuestion>.Success(200, "Get QaQuestion successfully", existingQaQuestion);
        }
    }
}
