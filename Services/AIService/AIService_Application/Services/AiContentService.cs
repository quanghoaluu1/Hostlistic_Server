using System.Diagnostics;
using System.Text.RegularExpressions;
using AIService_Application.DTOs.Requests;
using AIService_Application.DTOs.Responses;
using AIService_Application.Interface;
using AIService_Application.Prompts;
using AIService_Domain.Entities;
using AIService_Domain.Enum;
using AIService_Domain.Interfaces;
using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AIService_Application.Services;

public partial class AiContentService(
    IAiProvider aiProvider,
    IAiRequestRepository aiRequestRepository,
    IAiGeneratedContentRepository aiGeneratedContentRepository,
    IPromptTemplateRepository promptTemplateRepository,
    IPromptTemplateEngine promptTemplateEngine,
    IEventServiceClient eventServiceClient,
    ILogger<AiContentService> logger)
    : IAiContentService
{
    
    public async Task<ApiResponse<AiContentResponse>> GenerateDescriptionAsync(GenerateDescriptionRequest request, Guid userId, CancellationToken ct = default)
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
        var eventEntity = await eventServiceClient.GetEventByIdAsync(request.EventId, ct) ?? throw new Exception("Event not found");
        var template = promptTemplateRepository.GetByKeyAsync(PromptTemplateKey.EventDescription, ct).Result ?? throw new Exception("Event description prompt template not found");
        // var systemPrompt = PromptBuilder.BuildSystemPrompt(request.Language);
        // var userPrompt = PromptBuilder.BuildDescriptionPrompt(request);
        var systemPrompt = template.SystemPrompt;
        var userPrompt = template.UserPromptTemplate;
        var parameters = promptTemplateEngine.BuildParametersFromEvent(eventEntity);
        var parameterTone = promptTemplateEngine.AddToneAndLanguage(parameters, request.Tone, request.Language);
       
        if (!string.IsNullOrEmpty(request.TargetAudience))
            parameters["target_audience"] = request.TargetAudience;
        if (!string.IsNullOrEmpty(request.Objectives))
            parameters["objectives"] = request.Objectives;
        if (request.Keywords != null && request.Keywords.Count != 0)
            parameters["keywords"] = string.Join(", ", request.Keywords);
        if (!string.IsNullOrEmpty(request.AdditionalContext))
            parameters["additional_context"] = request.AdditionalContext;
        userPrompt = promptTemplateEngine.Render(userPrompt, parameterTone);
        var sw = Stopwatch.StartNew();

        try
        {
            logger.LogInformation("Generating description for event {EventId}, tone = {Tone}, language = {Language}", request.EventId, request.Tone, request.Language);
            var result = await aiProvider.GenerateContentAsync(systemPrompt, userPrompt, new AiRequestOptions()
            {
                MaxTokens = template.DefaultMaxTokens,
                Temperature = template.DefaultTemperature,
            }, ct);
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
            var aiContentResponse = new AiContentResponse()
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
            return new ApiResponse<AiContentResponse>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Description generated successfully",
                Data = aiContentResponse
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
            return ApiResponse<AiContentResponse>.Fail(500, "Failed to generate description");
        }   
    }

    public async Task<ApiResponse<EmailContentResponse>> GenerateEmailAsync(GenerateEmailRequest request, Guid organizerId,
        CancellationToken ct = default)
    {
        var eventEntity = await eventServiceClient.GetEventByIdAsync(request.EventId, ct);
        if (eventEntity is null)
            return ApiResponse<EmailContentResponse>.Fail(404, "Event not found");
        var templateType = request.EmailType switch
        {
            "invitation" => PromptTemplateKey.EmailInvitation,
            var r when r.StartsWith("reminder_") => PromptTemplateKey.EmailReminder,
            "post_event_thankyou" => PromptTemplateKey.PostEventThankyou,
            _ => throw new BadHttpRequestException($"Unknown email type: {request.EmailType}")
        };
        var template = await promptTemplateRepository.GetByKeyAsync(templateType, ct) ?? throw new Exception($"Template {templateType} not found");
        var parameters = BuildEmailParameters(eventEntity, request);
        var renderedUserPrompt = promptTemplateEngine.Render(template.UserPromptTemplate, parameters);

        var aiRequest = new AiRequest()
        {
            Id = Guid.NewGuid(),
            EventId = request.EventId,
            CreatedBy = organizerId,
            RequestType = AiRequesttype.GenerateEmailContent,
            Tone = request.Tone,
            Language = request.Language,
            TargetAudience = request.RecipientType ?? "general",
            Status = AiRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        aiRequestRepository.Add(aiRequest);
        await aiRequestRepository.SaveChangesAsync(ct);
        var sw = Stopwatch.StartNew();
        try
        {
            var aiResult = await aiProvider.GenerateContentAsync(
                template.SystemPrompt, renderedUserPrompt, new AiRequestOptions()
                {
                    Temperature = template.DefaultTemperature,
                    MaxTokens = template.DefaultMaxTokens,
                }, ct);
            sw.Stop();
            var (subjectLine, htmlBody) = ParseEmailResponse(aiResult.Content);
            var plainTextBody = StripHtmlTags(htmlBody);
            var aiContent = new AiGeneratedContent()
            {
                Id = Guid.NewGuid(),
                RequestId = aiRequest.Id,
                HtmlContent = htmlBody,
                PlainContent = plainTextBody,
                Model = aiResult.Model,
                CompletionTokens = aiResult.CompletionTokens,
                CreatedAt = DateTime.UtcNow,
                PromptTokens = aiResult.PromptTokens,
                LatencyMs = sw.ElapsedMilliseconds
            };
            aiGeneratedContentRepository.Add(aiContent);
            aiRequest.Status = AiRequestStatus.Completed;
            aiRequest.CompletedAt = DateTime.UtcNow;
            aiRequestRepository.Update(aiRequest);
            await aiRequestRepository.SaveChangesAsync(ct);
            var response = new EmailContentResponse()
            {
                ContentId = aiContent.Id,
                RequestId = aiRequest.Id,
                SubjectLine = subjectLine,
                HtmlBody = htmlBody,
                PlainTextBody = plainTextBody,
                EmailType = request.EmailType,
                Tone = request.Tone,
                Language = request.Language,
                Provider = aiContent.Model,
                Model = aiResult.Model,
                TokensUsed = aiResult.CompletionTokens,
                GenerationTimeMs = aiContent.LatencyMs
            };
            return new ApiResponse<EmailContentResponse>()
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Email content generated successfully",
                Data = response
            };
        }
        catch (Exception ex)
        {
            aiRequest.Status = AiRequestStatus.Failed;
            aiRequest.ErrorMessage = ex.Message;
            aiRequestRepository.Update(aiRequest);
            await aiRequestRepository.SaveChangesAsync(ct);
            logger.LogError(ex, "Failed to generate email content for event {EventId}", request.EventId);
            return ApiResponse<EmailContentResponse>.Fail(500, "Failed to generate email content");
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

    private Dictionary<string, string> BuildEmailParameters(EventDetailDto eventEntity, GenerateEmailRequest request)
    {
        var parameters = new Dictionary<string, string>()
        {
            // Auto-fill từ Event entity
            ["event_title"] = eventEntity.Title,
            ["event_type"] = eventEntity.EventTypeName.ToString(),
            ["event_date"] = eventEntity.StartDate.Value.ToString("MMMM dd, yyyy"),
            ["event_time"] = eventEntity.StartDate.Value.ToString("hh:mm tt"),
            ["event_location"] = eventEntity.Location ?? "TBD",
            ["event_mode"] = eventEntity.EventMode,
            ["registration_link"] = $"https://hostlistic.tech/events/{eventEntity.Id}/register",

            // Từ request
            ["tone"] = request.Tone,
            ["language"] = request.Language,
            ["recipient_type"] = request.RecipientType ?? "general attendees",
            ["target_audience"] = request.TargetAudience ?? "",
            ["selling_points"] = request.SellingPoints ?? "",
            ["reminder_type"] = request.EmailType.Replace("reminder_", ""),
        };
        var talents = eventEntity.Tracks.SelectMany(t => t.Sessions).SelectMany(t => t.Talents).Select(s => $"{s.Name} ({s.Type}").Distinct();
        parameters["talents"] = talents?.Any() is true ? string.Join(", ", talents) : "TBA";
        var topics = eventEntity.Tracks
            .Select(t => $"{t.Name}: {string.Join(", ", t.Sessions.Select(s => s.Title))}")
            .Take(5);

        parameters["key_topics"] = topics.Any()
            ? string.Join(" | ", topics)
            : "";
        
        // Optional fields
        if (!string.IsNullOrEmpty(request.EarlyBirdDeadline))
            parameters["early_bird_deadline"] = request.EarlyBirdDeadline;
        if (!string.IsNullOrEmpty(request.EarlyBirdDiscount))
            parameters["early_bird_discount"] = request.EarlyBirdDiscount;
        if (!string.IsNullOrEmpty(request.TicketPrice))
            parameters["ticket_price"] = request.TicketPrice;
        if (!string.IsNullOrEmpty(request.CheckinInstructions))
            parameters["checkin_instructions"] = request.CheckinInstructions;
        if (!string.IsNullOrEmpty(request.PreparationNotes))
            parameters["preparation_notes"] = request.PreparationNotes;
        if (!string.IsNullOrEmpty(request.AgendaHighlights))
            parameters["agenda_highlights"] = request.AgendaHighlights;
        if (!string.IsNullOrEmpty(request.AttendeeName))
            parameters["attendee_name"] = request.AttendeeName;
        if (!string.IsNullOrEmpty(request.TicketType))
            parameters["ticket_type"] = request.TicketType;

        return parameters;
    }

    private (string subject, string htmlBody) ParseEmailResponse(string rawContent)
    {
        var sanitized = SanitizeHtml(rawContent);
        var subjectMatch = MyRegex().Match(sanitized);
        var subject = subjectMatch.Success ? subjectMatch.Groups[1].Value.Trim() : "You're Invited!";
        var body = sanitized;
        if (subjectMatch.Success)
        {
            body = sanitized.Substring(subjectMatch.Index + subjectMatch.Length).Trim();
        }

        if (!body.TrimStart().StartsWith("<div>", StringComparison.OrdinalIgnoreCase))
        {
            body = $"<div>{body}</div>";
        }
        return (subject, body);
    }

    [GeneratedRegex(@"SUBJECT:\s*(.+?)(?:\n|<br|<\/?\w)", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex MyRegex();
}