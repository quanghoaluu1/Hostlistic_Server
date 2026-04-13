using EventService_Domain.Enums;

namespace EventService_Application.DTOs;

public class SurveyFormDto
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<SurveyQuestionDto> Questions { get; set; } = [];
    public SurveyFormStatus Status { get; set; }
    public int ResponseCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SurveyQuestionDto
{
    public int Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public SurveyQuestionType Type { get; set; }
    public List<SurveyQuestionOptionDto> Options { get; set; } = [];
    public bool IsRequired { get; set; }
    public int Order { get; set; }
}

public class SurveyQuestionOptionDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Order { get; set; }
}

public class SurveyPublicDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<SurveyQuestionDto> Questions { get; set; } = [];
    public bool HasResponded { get; set; }
}
public class CreateSurveyFormRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<CreateSurveyQuestionRequest> Questions { get; set; } = [];
}

public class UpdateSurveyFormRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<CreateSurveyQuestionRequest> Questions { get; set; } = [];
}
public class SubmitSurveyResponseRequest
{
    public List<SubmitSurveyAnswerRequest> Answers { get; set; } = [];
}
public class CreateSurveyQuestionRequest
{
    public string QuestionText { get; set; } = string.Empty;
    public SurveyQuestionType Type { get; set; }
    public List<string> Options { get; set; } = [];
    public bool IsRequired { get; set; } = true;
}
public class SubmitSurveyAnswerRequest
{
    public int QuestionId { get; set; }
    public List<int>? SelectedOptionIds { get; set; }
    public string? TextAnswer { get; set; }
}
public class SurveyResponseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public List<SurveyAnswerDto> Answers { get; set; } = [];
    public DateTime SubmittedAt { get; set; }
}

public class SurveyAnswerDto
{
    public int QuestionId { get; set; }
    public List<int>? SelectedOptionIds { get; set; }
    public string? TextAnswer { get; set; }
}
public class SurveySummaryDto
{
    public Guid SurveyFormId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int TotalResponses { get; set; }
    public List<QuestionSummaryDto> QuestionSummaries { get; set; } = [];
}

public class QuestionSummaryDto
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public List<OptionSummaryDto> OptionSummaries { get; set; } = [];
    public List<string> TextResponses { get; set; } = [];
}

public class OptionSummaryDto
{
    public int OptionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}