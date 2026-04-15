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

        public FeedbackService(IFeedbackRepository feedbackRepository, IEventRepository eventRepository)
        {
            _feedbackRepository = feedbackRepository;
            _eventRepository = eventRepository;
        }

        public async Task<ApiResponse<FeedbackDto>> AddFeedbackAsync(CreateFeedbackDto request, Guid userId, string userFullName)
        {
            var existingEvent = await _eventRepository.GetEventByIdAsync(request.EventId);
            if (existingEvent == null)
                return ApiResponse<FeedbackDto>.Fail(404, "Event not found.");

            var existing = await _feedbackRepository.GetFeedbackByEventAndUserAsync(request.EventId, userId);
            if (existing != null)
                return ApiResponse<FeedbackDto>.Fail(409, "You have already submitted feedback for this event.");

            var newFeedback = new Feedback
            {
                Id = Guid.NewGuid(),
                EventId = request.EventId,
                Rating = request.Rating,
                Comment = request.Comment,
                UserId = userId,
                UserFullName = userFullName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _feedbackRepository.AddFeedbackAsync(newFeedback);
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

        public async Task<ApiResponse<List<FeedbackDto>>> GetAllFeedbacksAsync()
        {
            var feedbacks = await _feedbackRepository.GetAllFeedbacksAsync();
            var feedbackDtos = feedbacks.Adapt<List<FeedbackDto>>();
            return ApiResponse<List<FeedbackDto>>.Success(200, "Retrieved all feedbacks successfully.", feedbackDtos);
        }

        public async Task<ApiResponse<List<FeedbackDto>>> GetFeedbacksByEventIdAsync(Guid eventId)
        {
            var feedbacks = await _feedbackRepository.GetFeedbacksByEventIdAsync(eventId);
            var feedbackDtos = feedbacks.Adapt<List<FeedbackDto>>();
            return ApiResponse<List<FeedbackDto>>.Success(200, "Retrieved feedbacks successfully.", feedbackDtos);
        }

        public async Task<ApiResponse<FeedbackDto?>> GetMyFeedbackForEventAsync(Guid eventId, Guid userId)
        {
            var feedback = await _feedbackRepository.GetFeedbackByEventAndUserAsync(eventId, userId);
            if (feedback == null)
                return ApiResponse<FeedbackDto?>.Success(204, "No feedback found.", null);

            var feedbackDto = feedback.Adapt<FeedbackDto>();
            return ApiResponse<FeedbackDto?>.Success(200, "Retrieved feedback successfully.", feedbackDto);
        }

        public async Task<ApiResponse<FeedbackDto>> UpdateFeedbackAsync(Guid id, UpdateFeedbackDto request, Guid userId)
        {
            var existingFeedback = await _feedbackRepository.GetFeedbackByIdAsync(id);
            if (existingFeedback == null)
                return ApiResponse<FeedbackDto>.Fail(404, "Feedback not found.");

            if (existingFeedback.UserId != userId)
                return ApiResponse<FeedbackDto>.Fail(403, "You can only update your own feedback.");

            existingFeedback.Rating = request.Rating;
            existingFeedback.Comment = request.Comment;
            existingFeedback.UpdatedAt = DateTime.UtcNow;

            await _feedbackRepository.UpdateFeedbackAsync(existingFeedback);
            var feedbackDto = existingFeedback.Adapt<FeedbackDto>();
            return ApiResponse<FeedbackDto>.Success(200, "Feedback updated successfully.", feedbackDto);
        }

        public async Task<ApiResponse<bool>> DeleteFeedbackAsync(Guid id, Guid userId)
        {
            var feedback = await _feedbackRepository.GetFeedbackByIdAsync(id);
            if (feedback == null)
                return ApiResponse<bool>.Fail(404, "Feedback not found.");

            if (feedback.UserId != userId)
                return ApiResponse<bool>.Fail(403, "You can only delete your own feedback.");

            var success = await _feedbackRepository.DeleteFeedbackAsync(id);
            if (!success)
                return ApiResponse<bool>.Fail(404, "Feedback not found.");

            return ApiResponse<bool>.Success(200, "Feedback deleted successfully.", true);
        }
    }
}
