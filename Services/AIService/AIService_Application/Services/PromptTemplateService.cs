using AIService_Application.DTOs.Requests;
using AIService_Application.DTOs.Responses;
using AIService_Application.Interface;
using AIService_Domain.Entities;
using AIService_Domain.Enum;
using AIService_Domain.Interfaces;
using Common;
using Microsoft.Extensions.Logging;

namespace AIService_Application.Services;

public class PromptTemplateService(
    IPromptTemplateRepository promptTemplateRepository,
    ILogger<PromptTemplateService> logger)
    : IPromptTemplateService
{
    public async Task<ApiResponse<IReadOnlyList<PromptTemplateResponse>>> GetAllAsync(CancellationToken ct = default)
    {
        var templates = await promptTemplateRepository.GetAllAsync(ct);
        var response = templates.Select(MapToResponse).ToList();
        return ApiResponse<IReadOnlyList<PromptTemplateResponse>>.Success(200, "OK", response);
    }

    public async Task<ApiResponse<IReadOnlyList<PromptTemplateResponse>>> GetByCategoryAsync(PromptCategory category, CancellationToken ct = default)
    {
        var templates = await promptTemplateRepository.GetByCategoryAsync(category, ct);
        var response = templates.Select(MapToResponse).ToList();
        return ApiResponse<IReadOnlyList<PromptTemplateResponse>>.Success(200, "OK", response);
    }

    public async Task<ApiResponse<PromptTemplateResponse>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var template = await promptTemplateRepository.GetByIdAsync(id, ct);
        if (template is null)
            return ApiResponse<PromptTemplateResponse>.Fail(404, "Prompt template not found");

        return ApiResponse<PromptTemplateResponse>.Success(200, "OK", MapToResponse(template));
    }

    public async Task<ApiResponse<PromptTemplateResponse>> GetByKeyAsync(PromptTemplateKey key, CancellationToken ct = default)
    {
        var template = await promptTemplateRepository.GetByKeyAsync(key, ct);
        if (template is null)
            return ApiResponse<PromptTemplateResponse>.Fail(404, "Prompt template not found");

        return ApiResponse<PromptTemplateResponse>.Success(200, "OK", MapToResponse(template));
    }

    public async Task<ApiResponse<PromptTemplateResponse>> CreateAsync(CreatePromptTemplateRequest request, CancellationToken ct = default)
    {
        var existing = await promptTemplateRepository.GetByKeyAsync(request.TemplateKey, ct);
        if (existing is not null)
            return ApiResponse<PromptTemplateResponse>.Fail(409, $"A prompt template with key '{request.TemplateKey}' already exists");

        var template = new PromptTemplate
        {
            Id = Guid.NewGuid(),
            TemplateKey = request.TemplateKey,
            DisplayName = request.DisplayName,
            Description = request.Description,
            Category = request.Category,
            SystemPrompt = request.SystemPrompt,
            UserPromptTemplate = request.UserPromptTemplate,
            DefaultTemperature = request.DefaultTemperature,
            DefaultMaxTokens = request.DefaultMaxTokens,
            IsActive = true,
            Version = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        promptTemplateRepository.Add(template);
        await promptTemplateRepository.SaveChangesAsync(ct);

        logger.LogInformation("Created PromptTemplate {Key} with ID {Id}", template.TemplateKey, template.Id);
        return ApiResponse<PromptTemplateResponse>.Success(201, "Prompt template created", MapToResponse(template));
    }

    public async Task<ApiResponse<PromptTemplateResponse>> UpdateAsync(Guid id, UpdatePromptTemplateRequest request, CancellationToken ct = default)
    {
        var template = await promptTemplateRepository.GetByIdAsync(id, ct);
        if (template is null)
            return ApiResponse<PromptTemplateResponse>.Fail(404, "Prompt template not found");

        if (request.DisplayName is not null) template.DisplayName = request.DisplayName;
        if (request.Description is not null) template.Description = request.Description;
        if (request.Category is not null) template.Category = request.Category.Value;
        if (request.SystemPrompt is not null) template.SystemPrompt = request.SystemPrompt;
        if (request.UserPromptTemplate is not null) template.UserPromptTemplate = request.UserPromptTemplate;
        if (request.DefaultTemperature is not null) template.DefaultTemperature = request.DefaultTemperature.Value;
        if (request.DefaultMaxTokens is not null) template.DefaultMaxTokens = request.DefaultMaxTokens.Value;
        if (request.IsActive is not null) template.IsActive = request.IsActive.Value;

        template.UpdatedAt = DateTime.UtcNow;
        template.Version++;

        promptTemplateRepository.Update(template);
        await promptTemplateRepository.SaveChangesAsync(ct);

        logger.LogInformation("Updated PromptTemplate {Key} (ID {Id}), version now {Version}", template.TemplateKey, template.Id, template.Version);
        return ApiResponse<PromptTemplateResponse>.Success(200, "Prompt template updated", MapToResponse(template));
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var template = await promptTemplateRepository.GetByIdAsync(id, ct);
        if (template is null)
            return ApiResponse<bool>.Fail(404, "Prompt template not found");

        template.IsActive = false;
        template.UpdatedAt = DateTime.UtcNow;

        promptTemplateRepository.Update(template);
        await promptTemplateRepository.SaveChangesAsync(ct);

        logger.LogInformation("Deactivated PromptTemplate {Key} (ID {Id})", template.TemplateKey, template.Id);
        return ApiResponse<bool>.Success(200, "Prompt template deactivated", true);
    }

    private static PromptTemplateResponse MapToResponse(PromptTemplate t) => new()
    {
        Id = t.Id,
        TemplateKey = t.TemplateKey,
        DisplayName = t.DisplayName,
        Description = t.Description,
        Category = t.Category,
        SystemPrompt = t.SystemPrompt,
        UserPromptTemplate = t.UserPromptTemplate,
        DefaultTemperature = t.DefaultTemperature,
        DefaultMaxTokens = t.DefaultMaxTokens,
        IsActive = t.IsActive,
        Version = t.Version,
        CreatedAt = t.CreatedAt,
        UpdatedAt = t.UpdatedAt,
    };
}
