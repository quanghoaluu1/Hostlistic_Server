namespace AIService_Application.DTOs.External;

/// <summary>
/// Mirrors EventService SurveyFormDto (list view with response counts).
/// Source: GET /api/events/{eventId}/surveys
/// </summary>
public class ExternalSurveyFormDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ResponseCount { get; set; }

    /// <summary>Draft | Published | Closed</summary>
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Mirrors EventService SurveySummaryDto (aggregated analytics).
/// Source: GET /api/events/{eventId}/surveys/{surveyId}/summary
/// </summary>
public class ExternalSurveySummaryDto
{
    public Guid SurveyFormId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int TotalResponses { get; set; }
    public List<ExternalQuestionSummaryDto> QuestionSummaries { get; set; } = [];
}

public class ExternalQuestionSummaryDto
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;

    /// <summary>Radio | Checkbox | TextInput</summary>
    public string Type { get; set; } = string.Empty;

    public List<ExternalOptionSummaryDto> OptionSummaries { get; set; } = [];

    /// <summary>Free-text answers for TextInput questions.</summary>
    public List<string> TextResponses { get; set; } = [];
}

public class ExternalOptionSummaryDto
{
    public int OptionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}
