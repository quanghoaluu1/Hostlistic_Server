using Common;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using EventService_Domain.Entities;
using EventService_Domain.Interfaces;
using Mapster;

namespace EventService_Application.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepository _feedbackRepository;
        private readonly IEventRepository _eventRepository;
        private ISessionRepository _sessionRepository;
        public FeedbackService(IFeedbackRepository feedbackRepository, IEventRepository eventRepository, ISessionRepository sessionRepository)
        {
            _feedbackRepository = feedbackRepository;
            _eventRepository = eventRepository;
            _sessionRepository = sessionRepository;
        }

        public async Task<ApiResponse<FeedbackDto>> AddFeedbackAsync(FeedbackDto request)
        {
            var existingEvent = await _eventRepository.GetEventByIdAsync(request.EventId);
            if (existingEvent == null)
                return ApiResponse<FeedbackDto>.Fail(404, "Event not found.");
            var existingSession = await _sessionRepository.GetSessionByIdAsync(request.SessionId);
            if (existingSession == null)
                return ApiResponse<FeedbackDto>.Fail(404, "Session not found.");
            if (request.Rating < 1 || request.Rating > 5)
                return ApiResponse<FeedbackDto>.Fail(400, "Rating must be between 1 and 5.");
            if (request.Comment != null && request.Comment.Length > 1000)
                return ApiResponse<FeedbackDto>.Fail(400, "Comment cannot exceed 1000 characters.");
            if (request.UserId == Guid.Empty)
                return ApiResponse<FeedbackDto>.Fail(400, "UserId is required.");
            if (string.IsNullOrEmpty(request.Comment))
                return ApiResponse<FeedbackDto>.Fail(400, "Comment is required.");
            var newFeedback = new Feedback
            {
                Id = new Guid(),
                EventId = request.EventId,
                SessionId = request.SessionId,
                Rating = request.Rating,
                Comment = request.Comment,
                UserId = request.UserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _feedbackRepository.AddFeddbackAsync(newFeedback);
            var feedbackDto = newFeedback.Adapt<FeedbackDto>();
            return ApiResponse<FeedbackDto>.Success(201, "Feedback added successfully.", feedbackDto);
        }

        public async Task<ApiResponse<FeedbackDto>> GetFeedbackByIdAsync(Guid id)
        {
            var feedback = await _feedbackRepository.GetFeedbackByIdAsync(id);
            if (feedback == null)
                return ApiResponse<FeedbackDto>.Fail(404, "Feedback not found.");
            var feedbackDto = feedback.Adapt<FeedbackDto>();
            return ApiResponse<FeedbackDto>.Success(200, "Retrieved feedback successfully.", feedbackDto);
        }

        public Task<ApiResponse<List<FeedbackDto>>> GetAllFeedbacksAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<List<FeedbackDto>>> GetFeedbacksByEventIdAsync(Guid eventId)
        {
            var feedbacks = await _feedbackRepository.GetFeedbacksByEventIdAsync(eventId);
            var feedbackDtos = feedbacks.Adapt<List<FeedbackDto>>();
            return ApiResponse<List<FeedbackDto>>.Success(200, "Retrieved feedbacks successfully.", feedbackDtos);
        }

        public async Task<ApiResponse<List<FeedbackDto>>> GetFeedbacksBySessionIdAsync(Guid sessionId)
        {
            var feedbacks = await _feedbackRepository.GetFeedbacksBySessionAsync(sessionId);
            var feedbackDtos = feedbacks.Adapt<List<FeedbackDto>>();
            return ApiResponse<List<FeedbackDto>>.Success(200, "Retrieved feedbacks successfully.", feedbackDtos);
        }

        public async Task<ApiResponse<List<FeedbackDto>>> GetAllFeedback()
        {
            var feedbacks = await _feedbackRepository.GetAllFeedbacksAsync();
            var feedbackDtos = feedbacks.Adapt<List<FeedbackDto>>();
            return ApiResponse<List<FeedbackDto>>.Success(200, "Retrieved all feedbacks successfully.", feedbackDtos);
        }

        public async Task<ApiResponse<FeedbackDto>> UpdateFeedbackAsync(Guid id, UpdateFeedbackDto request)
        {
            var existingFeedback = await _feedbackRepository.GetFeedbackByIdAsync(id);
            if (existingFeedback == null)
                return ApiResponse<FeedbackDto>.Fail(404, "Feedback not found.");
            if (request.Rating < 1 || request.Rating > 5)
                return ApiResponse<FeedbackDto>.Fail(400, "Rating must be between 1 and 5.");
            if (request.Comment != null && request.Comment.Length > 1000)
                return ApiResponse<FeedbackDto>.Fail(400, "Comment cannot exceed 1000 characters.");
            if (!string.IsNullOrEmpty(request.Comment))
                existingFeedback.Comment = request.Comment;
            if (request.Rating != request.Rating)
                existingFeedback.Rating = request.Rating;
            existingFeedback.UpdatedAt = DateTime.UtcNow;
            await _feedbackRepository.UpdateFeedbackAsync(existingFeedback);
            var feedbackDto = existingFeedback.Adapt<FeedbackDto>();
            return ApiResponse<FeedbackDto>.Success(200, "Feedback updated successfully.", feedbackDto);
        }

        public async Task<ApiResponse<bool>> DeleteFeedbackAsync(Guid id)
        {
            var success = await _feedbackRepository.DeleteFeedbackAsync(id);
            if (!success)
                return ApiResponse<bool>.Fail(404, "Feedback not found.");
            return ApiResponse<bool>.Success(200, "Feedback deleted successfully.", true);
        }
    }
}
