using AIService_Application.DTOs.Requests;
using AIService_Application.DTOs.Responses;

namespace AIService_Application.Interface;

public interface IAiContentService
{
    Task<AiContentResponse> GenerateDescriptionAsync(
        GenerateDescriptionRequest request,
        Guid userId,
        CancellationToken ct = default);
}