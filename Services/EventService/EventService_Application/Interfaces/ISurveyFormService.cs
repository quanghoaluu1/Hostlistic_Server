using Common;
using EventService_Application.DTOs;

namespace EventService_Application.Interfaces;

public interface ISurveyFormService
{
    // Organizer endpoints
    Task<ApiResponse<SurveyFormDto>> CreateSurveyAsync(Guid eventId, CreateSurveyFormRequest request, Guid organizerId);
    Task<ApiResponse<SurveyFormDto>> UpdateSurveyAsync(Guid eventId, Guid surveyId, UpdateSurveyFormRequest request, Guid organizerId);
    Task<ApiResponse<bool>> DeleteSurveyAsync(Guid eventId, Guid surveyId, Guid organizerId);
    Task<ApiResponse<bool>> PublishSurveyAsync(Guid eventId, Guid surveyId, Guid organizerId);
    Task<ApiResponse<bool>> CloseSurveyAsync(Guid eventId, Guid surveyId, Guid organizerId);
    Task<ApiResponse<List<SurveyFormDto>>> GetSurveysByEventAsync(Guid eventId, Guid organizerId);
    Task<ApiResponse<SurveyFormDto>> GetSurveyDetailAsync(Guid eventId, Guid surveyId, Guid organizerId);
    Task<ApiResponse<List<SurveyResponseDto>>> GetSurveyResponsesAsync(Guid eventId, Guid surveyId, Guid organizerId);
    Task<ApiResponse<SurveySummaryDto>> GetSurveySummaryAsync(Guid eventId, Guid surveyId, Guid organizerId);

    // Attendee endpoints
    Task<ApiResponse<SurveyPublicDto>> GetPublicSurveyAsync(Guid eventId, Guid surveyId, Guid userId);
    Task<ApiResponse<List<SurveyPublicDto>>> GetPublicSurveysByEventAsync(Guid eventId, Guid userId);
    Task<ApiResponse<bool>> SubmitSurveyResponseAsync(Guid eventId, Guid surveyId, SubmitSurveyResponseRequest request, Guid userId);
}