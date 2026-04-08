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

    public async Task<ApiResponse<AiContentResponse>> GenerateDescriptionAsync(GenerateDescriptionRequest request,
        Guid userId, CancellationToken ct = default)
    {
        var aiRequest = new AiRequest()
        {
            Id = Guid.NewGuid(),
            EventId = request.EventId,
            CreatedBy = userId,
            RequestType = AiRequestType.GenerateDescription,
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
        var eventEntity = await eventServiceClient.GetEventByIdAsync(request.EventId, ct) ??
                          throw new Exception("Event not found");
        var template = promptTemplateRepository.GetByKeyAsync(PromptTemplateKey.EventDescription, ct).Result ??
                       throw new Exception("Event description prompt template not found");
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
            logger.LogInformation("Generating description for event {EventId}, tone = {Tone}, language = {Language}",
                request.EventId, request.Tone, request.Language);
            var result = await aiProvider.GenerateContentAsync(systemPrompt, userPrompt, new AiRequestOptions()
            {
                MaxTokens = template.DefaultMaxTokens,
                Temperature = template.DefaultTemperature,
            }, ct);
            sw.Stop();

            var htmlContent = promptTemplateEngine.SanitizeHtml(result.Content);
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

    public async Task<ApiResponse<EmailContentResponse>> GenerateEmailAsync(GenerateEmailRequest request,
        Guid organizerId,
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
        var template = await promptTemplateRepository.GetByKeyAsync(templateType, ct) ??
                       throw new Exception($"Template {templateType} not found");
        var parameters = promptTemplateEngine.BuildEmailParameters(eventEntity, request);
        var renderedUserPrompt = promptTemplateEngine.Render(template.UserPromptTemplate, parameters);

        var aiRequest = new AiRequest()
        {
            Id = Guid.NewGuid(),
            EventId = request.EventId,
            CreatedBy = organizerId,
            RequestType = AiRequestType.GenerateEmailContent,
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
            var (subjectLine, htmlBody) = promptTemplateEngine.ParseEmailResponse(aiResult.Content);
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

    public async Task<ApiResponse<SocialPostResponse>> GenerateSocialPostAsync(GenerateSocialPostRequest request,
        Guid organizerId, CancellationToken ct = default)
    {
        var eventEntity = await eventServiceClient.GetEventByIdAsync(request.EventId, ct);
        if (eventEntity is null)
        {
            return ApiResponse<SocialPostResponse>.Fail(404, "Event not found");
        }

        var template = await promptTemplateRepository.GetByKeyAsync(PromptTemplateKey.SocialMediaPost, ct);
        if (template is null)
        {
            return ApiResponse<SocialPostResponse>.Fail(404, "Social media post template not found");
        }

        var parameters = promptTemplateEngine.BuildSocialPostParameters(eventEntity, request);
        var renderedUserPrompt = promptTemplateEngine.Render(template.UserPromptTemplate, parameters);

        var aiRequest = new AiRequest
        {
            Id = Guid.NewGuid(),
            EventId = request.EventId,
            CreatedBy = organizerId,
            RequestType = AiRequestType.GenerateSocialPost,
            Tone = request.Tone,
            Language = request.Language,
            TargetAudience = request.Platform,
            AdditionalContext = request.KeyHighlights,
            Status = AiRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        aiRequestRepository.Add(aiRequest);
        await aiRequestRepository.SaveChangesAsync(ct);

        var sw = Stopwatch.StartNew();
        try
        {
            logger.LogInformation("Generating social post for event {EventId}, tone = {Tone}, language = {Language}",
                request.EventId, request.Tone, request.Language);
            var aiResult = await aiProvider.GenerateContentAsync(
                template.SystemPrompt,
                renderedUserPrompt,
                new AiRequestOptions()
                {
                    Temperature = template.DefaultTemperature,
                    MaxTokens = template.DefaultMaxTokens,
                }, ct);
            sw.Stop();

            var (postContent, hashtags) =
                promptTemplateEngine.ParseSocialPostResponse(aiResult.Content, request.Hashtags);

            var fullText = string.IsNullOrEmpty(hashtags) ? postContent : $"{postContent} {hashtags}";
            var charCount = fullText.Length;
            var platformLimit = promptTemplateEngine.GetPlatformCharacterLimit(request.Platform);
            var exceedsLimit = platformLimit > 0 && charCount > platformLimit;

            var aiContent = new AiGeneratedContent
            {
                Id = Guid.NewGuid(),
                RequestId = aiRequest.Id,
                HtmlContent = postContent, // Social post is plain text, store in HtmlContent for consistency
                PlainContent = fullText,
                Model = aiResult.Model,
                PromptTokens = aiResult.PromptTokens,
                CompletionTokens = aiResult.CompletionTokens,
                LatencyMs = sw.ElapsedMilliseconds,
                CreatedAt = DateTime.UtcNow
            };
            aiGeneratedContentRepository.Add(aiContent);

            aiRequest.Status = AiRequestStatus.Completed;
            aiRequest.CompletedAt = DateTime.UtcNow;
            aiRequestRepository.Update(aiRequest);
            await aiRequestRepository.SaveChangesAsync(ct);

            var response = new SocialPostResponse
            {
                ContentId = aiContent.Id,
                RequestId = aiRequest.Id,
                PostContent = postContent,
                Hashtags = hashtags,
                Platform = request.Platform,
                Length = request.Length,
                Tone = request.Tone,
                Language = request.Language,
                CharacterCount = charCount,
                ExceedsLimit = exceedsLimit,
                Model = aiResult.Model,
                TokensUsed = aiResult.CompletionTokens,
                GenerationTimeMs = sw.ElapsedMilliseconds
            };

            return ApiResponse<SocialPostResponse>.Success(
                200, "Social post generated successfully", response);
        }
        catch (Exception ex)
        {
            sw.Stop();
            aiRequest.Status = AiRequestStatus.Failed;
            aiRequest.ErrorMessage = ex.Message;
            aiRequest.CompletedAt = DateTime.UtcNow;
            aiRequestRepository.Update(aiRequest);
            await aiRequestRepository.SaveChangesAsync(CancellationToken.None);

            logger.LogError(ex,
                "Failed to generate social post for event {EventId}, platform={Platform}",
                request.EventId, request.Platform);

            return ApiResponse<SocialPostResponse>.Fail(500,
                "Failed to generate social media post");
        }
    }

    public async Task<ApiResponse<AiContentResponse>> GenerateSpeakerIntroAsync(
    GenerateSpeakerIntroRequest request,
    Guid organizerId,
    CancellationToken ct = default)
{
    // ── Validate mode-specific requirement ──
    if (request.Mode == "summarize" && string.IsNullOrWhiteSpace(request.SourceText))
        return ApiResponse<AiContentResponse>.Fail(400,
            "SourceText is required when mode is 'summarize'");

    // ── Step 1: Fetch event (for event context in prompt) ──
    var eventEntity = await eventServiceClient.GetEventByIdAsync(request.EventId, ct);
    if (eventEntity is null)
        return ApiResponse<AiContentResponse>.Fail(404, "Event not found");

    // ── Step 2: Fetch lineup (replaces old Tracks traversal) ──
    var lineup = await eventServiceClient.GetEventLineupAsync(request.EventId, ct);
    if (lineup is null)
        return ApiResponse<AiContentResponse>.Fail(404, "Event lineup not found");

    // ── Step 3: Find the talent — search BOTH event-wide and session-specific ──
    var talent = lineup.EventWideTalents
        .FirstOrDefault(t => t.TalentId == request.TalentId);

    if (talent is null)
    {
        talent = lineup.SessionTalents
            .SelectMany(s => s.Talents)
            .FirstOrDefault(t => t.TalentId == request.TalentId);
    }

    if (talent is null)
        return ApiResponse<AiContentResponse>.Fail(404,
            $"Talent {request.TalentId} not found in event lineup");

    // ── Step 4: Find all sessions this talent appears in ──
    var talentSessionNames = lineup.SessionTalents
        .Where(s => s.Talents.Any(t => t.TalentId == request.TalentId))
        .Select(s => s.SessionTitle)
        .ToList();

    // Check if talent is event-wide (appears in eventWideTalents)
    var isEventWide = lineup.EventWideTalents
        .Any(t => t.TalentId == request.TalentId);

    // ── Step 5: Load prompt template ──
    var template = await promptTemplateRepository.GetByKeyAsync(
        PromptTemplateKey.SpeakerIntroduction, ct);
    if (template is null)
        return ApiResponse<AiContentResponse>.Fail(404,
            "Speaker introduction prompt template not found");

    // ── Step 6: Build parameters (mode-aware) ──
    var parameters = promptTemplateEngine.BuildSpeakerIntroParameters(
        eventEntity, talent, talentSessionNames, isEventWide, request);
    var renderedUserPrompt = promptTemplateEngine.Render(
        template.UserPromptTemplate, parameters);

    // ── Step 7: Assess data quality ──
    var dataQuality = request.Mode == "summarize"
        ? "rich"
        : (!string.IsNullOrWhiteSpace(talent.Bio) && !string.IsNullOrWhiteSpace(talent.Organization))
            ? "rich"
            : !string.IsNullOrWhiteSpace(talent.Bio) || !string.IsNullOrWhiteSpace(talent.Organization)
                ? "partial"
                : "minimal";

    var needsReview = request.Mode == "from_name" && dataQuality != "rich";

    // ── Steps 8-12: Create AiRequest, call AI, persist, return ──
    // (UNCHANGED from previous version — keep as-is)
    var aiRequest = new AiRequest
    {
        Id = Guid.NewGuid(),
        EventId = request.EventId,
        CreatedBy = organizerId,
        RequestType = AiRequestType.GenerateTalentBio,
        Tone = request.Tone,
        Language = request.Language,
        TargetAudience = request.Mode,
        AdditionalContext = request.Mode == "summarize"
            ? $"[SUMMARIZE] {request.SourceText?[..Math.Min(request.SourceText.Length, 200)]}..."
            : request.AdditionalContext,
        Status = AiRequestStatus.Pending,
        CreatedAt = DateTime.UtcNow
    };
    aiRequestRepository.Add(aiRequest);
    await aiRequestRepository.SaveChangesAsync(ct);

    var sw = Stopwatch.StartNew();
    try
    {
        logger.LogInformation(
            "Generating speaker intro for talent {TalentId}, mode={Mode}, event {EventId}",
            request.TalentId, request.Mode, request.EventId);

        var aiResult = await aiProvider.GenerateContentAsync(
            template.SystemPrompt,
            renderedUserPrompt,
            new AiRequestOptions
            {
                Temperature = template.DefaultTemperature,
                MaxTokens = template.DefaultMaxTokens,
            }, ct);
        sw.Stop();

        var htmlContent = promptTemplateEngine.SanitizeHtml(aiResult.Content);
        var plainContent = StripHtmlTags(aiResult.Content);

        var generatedContent = new AiGeneratedContent
        {
            Id = Guid.NewGuid(),
            RequestId = aiRequest.Id,
            HtmlContent = htmlContent,
            PlainContent = plainContent,
            Model = aiResult.Model,
            PromptTokens = aiResult.PromptTokens,
            CompletionTokens = aiResult.CompletionTokens,
            LatencyMs = sw.ElapsedMilliseconds,
            CreatedAt = DateTime.UtcNow
        };
        aiGeneratedContentRepository.Add(generatedContent);

        aiRequest.Status = AiRequestStatus.Completed;
        aiRequest.CompletedAt = DateTime.UtcNow;
        aiRequestRepository.Update(aiRequest);
        await aiRequestRepository.SaveChangesAsync(ct);

        var response = new AiContentResponse
        {
            RequestId = aiRequest.Id,
            ContentId = generatedContent.Id,
            HtmlContent = htmlContent,
            PlainContent = plainContent,
            IsAiGenerated = true,
            Metadata = new AiMetadataDto
            {
                Model = aiResult.Model,
                PromptTokens = aiResult.PromptTokens,
                CompletionTokens = aiResult.CompletionTokens,
                LatencyMs = sw.ElapsedMilliseconds,
                DataQuality = dataQuality,
                NeedsReview = needsReview
            }
        };

        return ApiResponse<AiContentResponse>.Success(
            200, "Speaker introduction generated successfully", response);
    }
    catch (Exception ex)
    {
        sw.Stop();
        aiRequest.Status = AiRequestStatus.Failed;
        aiRequest.ErrorMessage = ex.Message;
        aiRequest.CompletedAt = DateTime.UtcNow;
        aiRequestRepository.Update(aiRequest);
        await aiRequestRepository.SaveChangesAsync(CancellationToken.None);

        logger.LogError(ex,
            "Failed to generate speaker intro for talent {TalentId} in event {EventId}",
            request.TalentId, request.EventId);

        return ApiResponse<AiContentResponse>.Fail(500,
            "Failed to generate speaker introduction");
    }
}

    public async Task<ApiResponse<AiContentResponse>> GenerateSessionAbstractAsync(
        GenerateSessionAbstractRequest request,
        Guid organizerId,
        CancellationToken ct = default)
    {
        // ── Step 1: Fetch event ──
        var eventEntity = await eventServiceClient.GetEventByIdAsync(request.EventId, ct);
        if (eventEntity is null)
            return ApiResponse<AiContentResponse>.Fail(404, "Event not found");

        // ── Step 2: Find the specific session and its parent track ──
        TrackDetailDto? parentTrack = null;
        SessionDetailDto? targetSession = null;

        foreach (var track in eventEntity.Tracks)
        {
            var session = track.Sessions.FirstOrDefault(s => s.Id == request.SessionId);
            if (session is not null)
            {
                parentTrack = track;
                targetSession = session;
                break;
            }
        }

        if (targetSession is null || parentTrack is null)
            return ApiResponse<AiContentResponse>.Fail(404,
                $"Session {request.SessionId} not found in event {request.EventId}");

        // ── Step 3: Load prompt template ──
        var template = await promptTemplateRepository.GetByKeyAsync(
            PromptTemplateKey.SessionAbstract, ct);
        if (template is null)
            return ApiResponse<AiContentResponse>.Fail(404,
                "Session abstract prompt template not found");

        // ── Step 4: Build parameters and render prompt ──
        var parameters = promptTemplateEngine.BuildSessionAbstractParameters(
            eventEntity, targetSession, parentTrack, request);
        var renderedUserPrompt = promptTemplateEngine.Render(
            template.UserPromptTemplate, parameters);

        // ── Step 5: Create AiRequest record ──
        var aiRequest = new AiRequest
        {
            Id = Guid.NewGuid(),
            EventId = request.EventId,
            CreatedBy = organizerId,
            RequestType = AiRequestType.GenerateSessionAbstract,
            Tone = request.Tone,
            Language = request.Language,
            TargetAudience = request.TargetAudience,
            AdditionalContext = request.AdditionalContext,
            Status = AiRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        aiRequestRepository.Add(aiRequest);
        await aiRequestRepository.SaveChangesAsync(ct);

        var sw = Stopwatch.StartNew();
        try
        {
            logger.LogInformation(
                "Generating session abstract for session {SessionId} in event {EventId}, tone={Tone}",
                request.SessionId, request.EventId, request.Tone);

            var aiResult = await aiProvider.GenerateContentAsync(
                template.SystemPrompt,
                renderedUserPrompt,
                new AiRequestOptions
                {
                    Temperature = template.DefaultTemperature,
                    MaxTokens = template.DefaultMaxTokens,
                }, ct);
            sw.Stop();

            var htmlContent = promptTemplateEngine.SanitizeHtml(aiResult.Content);
            var plainContent = StripHtmlTags(aiResult.Content);

            var generatedContent = new AiGeneratedContent
            {
                Id = Guid.NewGuid(),
                RequestId = aiRequest.Id,
                HtmlContent = htmlContent,
                PlainContent = plainContent,
                Model = aiResult.Model,
                PromptTokens = aiResult.PromptTokens,
                CompletionTokens = aiResult.CompletionTokens,
                LatencyMs = sw.ElapsedMilliseconds,
                CreatedAt = DateTime.UtcNow
            };
            aiGeneratedContentRepository.Add(generatedContent);

            aiRequest.Status = AiRequestStatus.Completed;
            aiRequest.CompletedAt = DateTime.UtcNow;
            aiRequestRepository.Update(aiRequest);
            await aiRequestRepository.SaveChangesAsync(ct);

            var response = new AiContentResponse
            {
                RequestId = aiRequest.Id,
                ContentId = generatedContent.Id,
                HtmlContent = htmlContent,
                PlainContent = plainContent,
                IsAiGenerated = true,
                Metadata = new AiMetadataDto
                {
                    Model = aiResult.Model,
                    PromptTokens = aiResult.PromptTokens,
                    CompletionTokens = aiResult.CompletionTokens,
                    LatencyMs = sw.ElapsedMilliseconds
                }
            };

            return ApiResponse<AiContentResponse>.Success(
                200, "Session abstract generated successfully", response);
        }
        catch (Exception ex)
        {
            sw.Stop();
            aiRequest.Status = AiRequestStatus.Failed;
            aiRequest.ErrorMessage = ex.Message;
            aiRequest.CompletedAt = DateTime.UtcNow;
            aiRequestRepository.Update(aiRequest);
            await aiRequestRepository.SaveChangesAsync(CancellationToken.None);

            logger.LogError(ex,
                "Failed to generate session abstract for session {SessionId} in event {EventId}",
                request.SessionId, request.EventId);

            return ApiResponse<AiContentResponse>.Fail(500,
                "Failed to generate session abstract");
        }
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
