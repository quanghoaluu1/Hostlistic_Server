using AIService_Application.DTOs.Requests;
using AIService_Application.DTOs.Responses;
using AIService_Domain.Enum;
using Common;

namespace AIService_Application.Interface;

public interface IPromptTemplateService
{
    Task<ApiResponse<IReadOnlyList<PromptTemplateResponse>>> GetAllAsync(CancellationToken ct = default);
    Task<ApiResponse<IReadOnlyList<PromptTemplateResponse>>> GetByCategoryAsync(PromptCategory category, CancellationToken ct = default);
    Task<ApiResponse<PromptTemplateResponse>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<PromptTemplateResponse>> GetByKeyAsync(PromptTemplateKey key, CancellationToken ct = default);
    Task<ApiResponse<PromptTemplateResponse>> CreateAsync(CreatePromptTemplateRequest request, CancellationToken ct = default);
    Task<ApiResponse<PromptTemplateResponse>> UpdateAsync(Guid id, UpdatePromptTemplateRequest request, CancellationToken ct = default);
    Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken ct = default);
}
