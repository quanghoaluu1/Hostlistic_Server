using System.Diagnostics;
using System.Text.RegularExpressions;
using AIService_Application.DTOs.Requests;
using AIService_Application.DTOs.Responses;
using AIService_Application.Interface;
using AIService_Application.Prompts;
using AIService_Domain.Entities;
using AIService_Domain.Enum;
using AIService_Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AIService_Application.Services;

public class AiContentService(
    IAiProvider aiProvider,
    IAiRequestRepository aiRequestRepository,
    IAiGeneratedContentRepository aiGeneratedContentRepository,
    ILogger<AiContentService> logger)
    : IAiContentService
{
    
    public async Task<AiContentResponse> GenerateDescriptionAsync(GenerateDescriptionRequest request, Guid userId, CancellationToken ct = default)
    {
        var aiRequest = new AiRequest()
        {
            Id = Guid.NewGuid(),
            EventId = request.EventId,
            CreatedBy = userId,
            RequestType = AiRequesttype.GenerateDescription,
            Tone = request.Tone,
            Language = request.Language,
            TargetAudience = request.TargetAudience,
            Objectives = request.Objectives,
            Keywords = request.Keywords != null ? string.Join(",", request.Keywords) : null,
            AdditionalContext = request.AdditionalContext,
            Status = AiRequestStatus.Pending,
        };
        aiRequestRepository.Add(aiRequest);
        await aiRequestRepository.SaveChangesAsync(ct);

        var systemPrompt = PromptBuilder.BuildSystemPrompt(request.Language);
        var userPrompt = PromptBuilder.BuildDescriptionPrompt(request);
        var sw = Stopwatch.StartNew();

        try
        {
            logger.LogInformation("Generating description for event {EventId}, tone = {Tone}, language = {Language}", request.EventId, request.Tone, request.Language);
            var result = await aiProvider.GenerateContentAsync(systemPrompt, userPrompt, ct);
            sw.Stop();
            
            var htmlContent = SanitizeHtml(result.Content);
            var plainContent = StripHtmlTags(result.Content);

            var generatedContent = new AiGeneratedContent()
            {
                Id = Guid.NewGuid(),
                RequestId = aiRequest.Id,
                HtmlContent = htmlContent,
                PlainContent = plainContent,
                Model = result.Model,
                PromptTokens = result.PromptTokens,
                CompletionTokens = result.CompletionTokens,
                LatencyMs = sw.ElapsedMilliseconds
            };
            aiGeneratedContentRepository.Add(generatedContent);
            aiRequest.Status = AiRequestStatus.Completed;
            aiRequest.CompletedAt = DateTime.UtcNow;
            await aiRequestRepository.SaveChangesAsync(ct);
            return new AiContentResponse()
            {
                RequestId = aiRequest.Id,
                ContentId = generatedContent.Id,
                HtmlContent = htmlContent,
                PlainContent = plainContent,
                IsAiGenerated = true,
                Metadata = new AiMetadataDto()
                {
                    Model = result.Model,
                    PromptTokens = result.PromptTokens,
                    CompletionTokens = result.CompletionTokens,
                    LatencyMs = sw.ElapsedMilliseconds
                }
            };

        }
        catch (Exception ex)
        {
            sw.Stop();
            aiRequest.Status = AiRequestStatus.Failed;
            aiRequest.ErrorMessage = ex.Message;
            aiRequest.CompletedAt = DateTime.UtcNow;
                await aiRequestRepository.SaveChangesAsync(CancellationToken.None);
            logger.LogError(ex, "Failed to generate description for event {EventId}", request.EventId);
            throw;
        }   
    }

    private static string SanitizeHtml(string raw)
    {
        var cleaned = Regex.Replace(raw, @"^```html?\s*\n?", "", RegexOptions.Multiline);
        cleaned = Regex.Replace(cleaned, @"\n?```\s*$", "", RegexOptions.Multiline);
        cleaned = Regex.Replace(cleaned, @"<\/?(html|head|body|div)[^>]*>", "",
            RegexOptions.IgnoreCase);
        cleaned = Regex.Replace(cleaned, @">\s+<", "><");
        return cleaned.Trim();
    }
    private static string StripHtmlTags(string html)
    {
        var text = Regex.Replace(html, @"<br\s*/?>", "\n");
        text = Regex.Replace(text, @"</?(p|h[1-6]|li|ul|ol)[^>]*>", "\n");
        text = Regex.Replace(text, @"<[^>]+>", "");
        text = Regex.Replace(text, @"\n{3,}", "\n\n");
        return text.Trim();
    }
}