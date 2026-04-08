using AIService_Application.DTOs.Requests;
using AIService_Application.DTOs.Responses;
using Common;

namespace AIService_Application.Interface;

public interface IAiContentService
{
    Task<ApiResponse<AiContentResponse>>GenerateDescriptionAsync(
        GenerateDescriptionRequest request,
        Guid userId,
        CancellationToken ct = default);

    Task<ApiResponse<EmailContentResponse>> GenerateEmailAsync(GenerateEmailRequest request, Guid organizerId,
        CancellationToken ct = default);
    
    Task<ApiResponse<SocialPostResponse>> GenerateSocialPostAsync(
        GenerateSocialPostRequest request,
        Guid organizerId,
        CancellationToken ct = default);
    
    Task<ApiResponse<AiContentResponse>> GenerateSpeakerIntroAsync(
        GenerateSpeakerIntroRequest request,
        Guid organizerId,
        CancellationToken ct = default);

    Task<ApiResponse<AiContentResponse>> GenerateSessionAbstractAsync(
        GenerateSessionAbstractRequest request,
        Guid organizerId,
        CancellationToken ct = default);
}