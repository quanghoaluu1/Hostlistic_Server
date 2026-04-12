using Common;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using EventService_Domain.Entities;
using EventService_Domain.Enums;
using EventService_Domain.Interfaces;
using Mapster;

namespace EventService_Application.Services;

public class SurveyFormService(
    ISurveyFormRepository surveyFormRepository,
    IEventRepository eventRepository
    ) : ISurveyFormService
{
    // ═══════════════════════════════════════════════
    // ORGANIZER ENDPOINTS
    // ═══════════════════════════════════════════════

    public async Task<ApiResponse<SurveyFormDto>> CreateSurveyAsync(
        Guid eventId, CreateSurveyFormRequest request, Guid organizerId)
    {
        var (ev, error) = await ValidateOrganizerAccessAsync<SurveyFormDto>(eventId, organizerId);
        if (error is not null) return error;

        if (string.IsNullOrWhiteSpace(request.Title))
            return ApiResponse<SurveyFormDto>.Fail(400, "Survey title is required.");

        var questionError = ValidateQuestions<SurveyFormDto>(request.Questions);
        if (questionError is not null) return questionError;

        var surveyForm = new SurveyForm
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Questions = NormalizeQuestions(request.Questions),
            Status = SurveyFormStatus.Draft
        };

        await surveyFormRepository.AddAsync(surveyForm);

        var dto = await MapToDto(surveyForm);
        return ApiResponse<SurveyFormDto>.Success(201, "Survey created successfully.", dto);
    }
    
    public async Task<ApiResponse<SurveyFormDto>> UpdateSurveyAsync(
        Guid eventId, Guid surveyId, UpdateSurveyFormRequest request, Guid organizerId)
    {
        var (_, error) = await ValidateOrganizerAccessAsync<SurveyFormDto>(eventId, organizerId);
        if (error is not null) return error;

        // Need tracked entity for update
        var form = await surveyFormRepository.GetByIdWithResponsesAsync(surveyId);
        if (form is null || form.EventId != eventId)
            return ApiResponse<SurveyFormDto>.Fail(404, "Survey not found.");

        if (form.Status != SurveyFormStatus.Draft)
            return ApiResponse<SurveyFormDto>.Fail(400, "Only draft surveys can be edited.");

        if (string.IsNullOrWhiteSpace(request.Title))
            return ApiResponse<SurveyFormDto>.Fail(400, "Survey title is required.");

        var questionError = ValidateQuestions<SurveyFormDto>(request.Questions);
        if (questionError is not null) return questionError;

        form.Title = request.Title.Trim();
        form.Description = request.Description?.Trim();
        form.Questions = NormalizeQuestions(request.Questions);

        surveyFormRepository.Update(form);
        await surveyFormRepository.SaveChangesAsync();

        var dto = await MapToDto(form);
        return ApiResponse<SurveyFormDto>.Success(200, "Survey updated successfully.", dto);
    }
    
    public async Task<ApiResponse<bool>> DeleteSurveyAsync(
        Guid eventId, Guid surveyId, Guid organizerId)
    {
        var (_, error) = await ValidateOrganizerAccessAsync<bool>(eventId, organizerId);
        if (error is not null) return error;

        var form = await surveyFormRepository.GetByIdAsync(surveyId);
        if (form is null || form.EventId != eventId)
            return ApiResponse<bool>.Fail(404, "Survey not found.");

        if (form.Status != SurveyFormStatus.Draft)
            return ApiResponse<bool>.Fail(400, "Only draft surveys can be deleted.");

        surveyFormRepository.Delete(form);
        await surveyFormRepository.SaveChangesAsync();

        return ApiResponse<bool>.Success(200, "Survey deleted successfully.", true);
    }
    
    public async Task<ApiResponse<bool>> PublishSurveyAsync(
        Guid eventId, Guid surveyId, Guid organizerId)
    {
        var (ev, error) = await ValidateOrganizerAccessAsync<bool>(eventId, organizerId);
        if (error is not null) return error;

        // Enforce: only publish after event completed
        if (ev!.EventStatus != EventStatus.Completed)
            return ApiResponse<bool>.Fail(400, "Survey can only be published after the event is completed.");

        var form = await surveyFormRepository.GetByIdWithResponsesAsync(surveyId);
        if (form is null || form.EventId != eventId)
            return ApiResponse<bool>.Fail(404, "Survey not found.");

        if (form.Status != SurveyFormStatus.Draft)
            return ApiResponse<bool>.Fail(400, "Only draft surveys can be published.");

        if (form.Questions.Count == 0)
            return ApiResponse<bool>.Fail(400, "Cannot publish a survey with no questions.");

        form.Status = SurveyFormStatus.Published;
        surveyFormRepository.Update(form);
        await surveyFormRepository.SaveChangesAsync();

        return ApiResponse<bool>.Success(200, "Survey published successfully.", true);
    }

    public async Task<ApiResponse<bool>> CloseSurveyAsync(
        Guid eventId, Guid surveyId, Guid organizerId)
    {
        var (_, error) = await ValidateOrganizerAccessAsync<bool>(eventId, organizerId);
        if (error is not null) return error;

        var form = await surveyFormRepository.GetByIdWithResponsesAsync(surveyId);
        if (form is null || form.EventId != eventId)
            return ApiResponse<bool>.Fail(404, "Survey not found.");

        if (form.Status != SurveyFormStatus.Published)
            return ApiResponse<bool>.Fail(400, "Only published surveys can be closed.");

        form.Status = SurveyFormStatus.Closed;
        surveyFormRepository.Update(form);
        await surveyFormRepository.SaveChangesAsync();

        return ApiResponse<bool>.Success(200, "Survey closed successfully.", true);
    }

    public async Task<ApiResponse<List<SurveyFormDto>>> GetSurveysByEventAsync(
        Guid eventId, Guid organizerId)
    {
        var (_, error) = await ValidateOrganizerAccessAsync<List<SurveyFormDto>>(eventId, organizerId);
        if (error is not null) return error;

        var forms = await surveyFormRepository.GetByEventIdAsync(eventId);
        var dtos = new List<SurveyFormDto>();

        foreach (var form in forms)
        {
            dtos.Add(await MapToDto(form));
        }

        return ApiResponse<List<SurveyFormDto>>.Success(200, "Surveys retrieved successfully.", dtos);
    }
    
    public async Task<ApiResponse<SurveyFormDto>> GetSurveyDetailAsync(
        Guid eventId, Guid surveyId, Guid organizerId)
    {
        var (_, error) = await ValidateOrganizerAccessAsync<SurveyFormDto>(eventId, organizerId);
        if (error is not null) return error;

        var form = await surveyFormRepository.GetByIdAsync(surveyId);
        if (form is null || form.EventId != eventId)
            return ApiResponse<SurveyFormDto>.Fail(404, "Survey not found.");

        var dto = await MapToDto(form);
        return ApiResponse<SurveyFormDto>.Success(200, "Survey retrieved successfully.", dto);
    }
    
    public async Task<ApiResponse<List<SurveyResponseDto>>> GetSurveyResponsesAsync(
        Guid eventId, Guid surveyId, Guid organizerId)
    {
        var (_, error) = await ValidateOrganizerAccessAsync<List<SurveyResponseDto>>(eventId, organizerId);
        if (error is not null) return error;

        var form = await surveyFormRepository.GetByIdAsync(surveyId);
        if (form is null || form.EventId != eventId)
            return ApiResponse<List<SurveyResponseDto>>.Fail(404, "Survey not found.");

        var responses = await surveyFormRepository.GetResponsesBySurveyIdAsync(surveyId);
        var dtos = responses.Adapt<List<SurveyResponseDto>>();

        return ApiResponse<List<SurveyResponseDto>>.Success(200, "Responses retrieved successfully.", dtos);
    }

    public async Task<ApiResponse<SurveySummaryDto>> GetSurveySummaryAsync(
        Guid eventId, Guid surveyId, Guid organizerId)
    {
        var (_, error) = await ValidateOrganizerAccessAsync<SurveySummaryDto>(eventId, organizerId);
        if (error is not null) return error;

        var form = await surveyFormRepository.GetByIdAsync(surveyId);
        if (form is null || form.EventId != eventId)
            return ApiResponse<SurveySummaryDto>.Fail(404, "Survey not found.");

        var responses = await surveyFormRepository.GetResponsesBySurveyIdAsync(surveyId);

        var summary = new SurveySummaryDto
        {
            SurveyFormId = form.Id,
            Title = form.Title,
            TotalResponses = responses.Count,
            QuestionSummaries = form.Questions.Select(q =>
            {
                var questionSummary = new QuestionSummaryDto
                {
                    QuestionId = q.Id,
                    QuestionText = q.QuestionText,
                    Type = q.Type.ToString()
                };

                if (q.Type is SurveyQuestionType.Radio or SurveyQuestionType.Checkbox)
                {
                    // Count selections per option
                    var optionCounts = q.Options.ToDictionary(o => o.Id, _ => 0);
                    var totalSelections = 0;

                    foreach (var r in responses)
                    {
                        var answer = r.Answers.FirstOrDefault(a => a.QuestionId == q.Id);
                        if (answer?.SelectedOptionIds is null) continue;

                        foreach (var optId in answer.SelectedOptionIds)
                        {
                            if (optionCounts.ContainsKey(optId))
                            {
                                optionCounts[optId]++;
                                totalSelections++;
                            }
                        }
                    }

                    questionSummary.OptionSummaries = q.Options.Select(o => new OptionSummaryDto
                    {
                        OptionId = o.Id,
                        Text = o.Text,
                        Count = optionCounts.GetValueOrDefault(o.Id, 0),
                        Percentage = totalSelections > 0
                            ? Math.Round((double)optionCounts.GetValueOrDefault(o.Id, 0) / totalSelections * 100, 1)
                            : 0
                    }).ToList();
                }
                else if (q.Type == SurveyQuestionType.TextInput)
                {
                    questionSummary.TextResponses = responses
                        .Select(r => r.Answers.FirstOrDefault(a => a.QuestionId == q.Id)?.TextAnswer)
                        .Where(t => !string.IsNullOrWhiteSpace(t))
                        .ToList()!;
                }

                return questionSummary;
            }).ToList()
        };

        return ApiResponse<SurveySummaryDto>.Success(200, "Summary retrieved successfully.", summary);
    }
    
    // ═══════════════════════════════════════════════
    // ATTENDEE ENDPOINTS
    // ═══════════════════════════════════════════════

    public async Task<ApiResponse<List<SurveyPublicDto>>> GetPublicSurveysByEventAsync(
        Guid eventId, Guid userId)
    {
        var ev = await eventRepository.GetEventByIdAsync(eventId);
        if (ev is null)
            return ApiResponse<List<SurveyPublicDto>>.Fail(404, "Event not found.");

        var forms = await surveyFormRepository.GetByEventIdAsync(eventId);

        // Attendees only see Published surveys
        var publishedForms = forms.Where(f => f.Status == SurveyFormStatus.Published).ToList();

        var dtos = new List<SurveyPublicDto>();
        foreach (var form in publishedForms)
        {
            var existing = await surveyFormRepository.GetResponseAsync(form.Id, userId);
            var dto = form.Adapt<SurveyPublicDto>();
            dto.HasResponded = existing is not null;
            dtos.Add(dto);
        }

        return ApiResponse<List<SurveyPublicDto>>.Success(200, "Surveys retrieved.", dtos);
    }

    public async Task<ApiResponse<SurveyPublicDto>> GetPublicSurveyAsync(
        Guid eventId, Guid surveyId, Guid userId)
    {
        var form = await surveyFormRepository.GetByIdAsync(surveyId);
        if (form is null || form.EventId != eventId)
            return ApiResponse<SurveyPublicDto>.Fail(404, "Survey not found.");

        if (form.Status != SurveyFormStatus.Published)
            return ApiResponse<SurveyPublicDto>.Fail(404, "Survey not found.");

        var existing = await surveyFormRepository.GetResponseAsync(surveyId, userId);
        var dto = form.Adapt<SurveyPublicDto>();
        dto.HasResponded = existing is not null;

        return ApiResponse<SurveyPublicDto>.Success(200, "Survey retrieved.", dto);
    }

    public async Task<ApiResponse<bool>> SubmitSurveyResponseAsync(
        Guid eventId, Guid surveyId, SubmitSurveyResponseRequest request, Guid userId)
    {
        var form = await surveyFormRepository.GetByIdAsync(surveyId);
        if (form is null || form.EventId != eventId)
            return ApiResponse<bool>.Fail(404, "Survey not found.");

        if (form.Status != SurveyFormStatus.Published)
            return ApiResponse<bool>.Fail(400, "This survey is not accepting responses.");

        // Check duplicate submission
        var existing = await surveyFormRepository.GetResponseAsync(surveyId, userId);
        if (existing is not null)
            return ApiResponse<bool>.Fail(409, "You have already submitted this survey.");

        // Validate answers against questions
        var validationError = ValidateAnswers(form.Questions, request.Answers);
        if (validationError is not null)
            return validationError;

        var response = new SurveyResponse
        {
            Id = Guid.NewGuid(),
            SurveyFormId = surveyId,
            UserId = userId,
            SubmittedAt = DateTime.UtcNow,
            Answers = request.Answers.Select(a => new SurveyAnswer
            {
                QuestionId = a.QuestionId,
                SelectedOptionIds = a.SelectedOptionIds,
                TextAnswer = a.TextAnswer?.Trim()
            }).ToList()
        };

        await surveyFormRepository.AddResponseAsync(response);
        await surveyFormRepository.SaveChangesAsync();

        return ApiResponse<bool>.Success(201, "Survey submitted successfully.", true);
    }

    
    private async Task<(Event? ev, ApiResponse<T>? error)> ValidateOrganizerAccessAsync<T>(Guid eventId,
        Guid organizerId)
    {
        var ev = await eventRepository.GetEventByIdAsync(eventId);
        if (ev is null)
            return (null, ApiResponse<T>.Fail(404, "Event not found."));

        if (ev.OrganizerId != organizerId)
            return (null, ApiResponse<T>.Fail(403, "You are not the organizer of this event."));

        return (ev, null);
    }
    
    private static ApiResponse<T>? ValidateQuestions<T>(List<CreateSurveyQuestionRequest> questions)
    {
        if (questions.Count == 0)
            return ApiResponse<T>.Fail(400, "At least one question is required.");

        for (var i = 0; i < questions.Count; i++)
        {
            var q = questions[i];

            if (string.IsNullOrWhiteSpace(q.QuestionText))
                return ApiResponse<T>.Fail(400, $"Question {i + 1}: question text is required.");

            if (q.Type is SurveyQuestionType.Radio or SurveyQuestionType.Checkbox)
            {
                if (q.Options.Count < 2)
                    return ApiResponse<T>.Fail(400, $"Question {i + 1}: at least two options are required for {q.Type}.");

                if (q.Options.Any(string.IsNullOrWhiteSpace))
                    return ApiResponse<T>.Fail(400, $"Question {i + 1}: option text cannot be empty.");

                if (q.Options.Count != q.Options.Distinct(StringComparer.OrdinalIgnoreCase).Count())
                    return ApiResponse<T>.Fail(400, $"Question {i + 1}: duplicate options are not allowed.");
            }

            if (q.Type == SurveyQuestionType.TextInput && q.Options.Count > 0)
                return ApiResponse<T>.Fail(400, $"Question {i + 1}: TextInput question cannot have options.");
        }

        return null;
    }
    
    private static List<SurveyQuestion> NormalizeQuestions(List<CreateSurveyQuestionRequest> questions)
    {
        return questions.Select((q, qIndex) => new SurveyQuestion
        {
            Id = qIndex,
            QuestionText = q.QuestionText.Trim(),
            Type = q.Type,
            IsRequired = q.IsRequired,
            Order = qIndex,
            Options = q.Type == SurveyQuestionType.TextInput
                ? []
                : q.Options.Select((opt, oIndex) => new SurveyQuestionOption
                {
                    Id = oIndex,
                    Text = opt.Trim(),
                    Order = oIndex
                }).ToList()
        }).ToList();
    }
    private async Task<SurveyFormDto> MapToDto(SurveyForm form)
    {
        var dto = form.Adapt<SurveyFormDto>();
        dto.ResponseCount = await surveyFormRepository.GetResponseCountAsync(form.Id);
        return dto;
    }
    
    private static ApiResponse<bool>? ValidateAnswers(
        List<SurveyQuestion> questions, List<SubmitSurveyAnswerRequest> answers)
    {
        var answerMap = answers.ToDictionary(a => a.QuestionId);

        foreach (var q in questions)
        {
            var hasAnswer = answerMap.TryGetValue(q.Id, out var answer);

            // Check required
            if (q.IsRequired && !hasAnswer)
                return ApiResponse<bool>.Fail(400, $"Question '{q.QuestionText}' is required.");

            if (!hasAnswer) continue;

            switch (q.Type)
            {
                case SurveyQuestionType.Radio:
                {
                    if (answer!.SelectedOptionIds is null || answer.SelectedOptionIds.Count != 1)
                        return ApiResponse<bool>.Fail(400, $"Question '{q.QuestionText}' requires exactly one selection.");

                    var optionId = answer.SelectedOptionIds[0];
                    if (q.Options.All(o => o.Id != optionId))
                        return ApiResponse<bool>.Fail(400, $"Question '{q.QuestionText}': invalid option selected.");

                    break;
                }
                case SurveyQuestionType.Checkbox:
                {
                    if (q.IsRequired && (answer!.SelectedOptionIds is null || answer.SelectedOptionIds.Count == 0))
                        return ApiResponse<bool>.Fail(400, $"Question '{q.QuestionText}' requires at least one selection.");

                    if (answer!.SelectedOptionIds is not null)
                    {
                        var validIds = q.Options.Select(o => o.Id).ToHashSet();
                        if (answer.SelectedOptionIds.Any(id => !validIds.Contains(id)))
                            return ApiResponse<bool>.Fail(400, $"Question '{q.QuestionText}': invalid option selected.");

                        if (answer.SelectedOptionIds.Count != answer.SelectedOptionIds.Distinct().Count())
                            return ApiResponse<bool>.Fail(400, $"Question '{q.QuestionText}': duplicate selections are not allowed.");
                    }

                    break;
                }
                case SurveyQuestionType.TextInput:
                {
                    if (q.IsRequired && string.IsNullOrWhiteSpace(answer!.TextAnswer))
                        return ApiResponse<bool>.Fail(400, $"Question '{q.QuestionText}' requires a text answer.");

                    break;
                }
            }
        }

        // Check for answers to non-existent questions
        var validQuestionIds = questions.Select(q => q.Id).ToHashSet();
        if (answers.Any(a => !validQuestionIds.Contains(a.QuestionId)))
            return ApiResponse<bool>.Fail(400, "Answer references a question that does not exist in this survey.");

        return null;
    }
}