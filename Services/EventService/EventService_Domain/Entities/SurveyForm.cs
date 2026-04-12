using System.ComponentModel.DataAnnotations.Schema;
using Common;
using EventService_Domain.Enums;

namespace EventService_Domain.Entities;

public class SurveyForm : BaseClass
{
    public Guid EventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    [Column(TypeName = "jsonb")]
    public List<SurveyQuestion> Questions { get; set; } = [];

    public SurveyFormStatus Status { get; set; } = SurveyFormStatus.Draft;

    // Navigation
    [ForeignKey("EventId")]
    public virtual Event Event { get; set; } = null!;

    public ICollection<SurveyResponse> Responses { get; set; } = new List<SurveyResponse>();
}
public class SurveyQuestion
{
    public int Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public SurveyQuestionType Type { get; set; }
    public List<SurveyQuestionOption> Options { get; set; } = [];
    public bool IsRequired { get; set; } = true;
    public int Order { get; set; }
}
public class SurveyQuestionOption
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Order { get; set; }
}