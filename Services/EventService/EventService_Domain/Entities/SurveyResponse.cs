using System.ComponentModel.DataAnnotations.Schema;

namespace EventService_Domain.Entities;

public class SurveyResponse
{
    public Guid Id { get; set; }
    public Guid SurveyFormId { get; set; }
    public Guid UserId { get; set; }

    [Column(TypeName = "jsonb")]
    public List<SurveyAnswer> Answers { get; set; } = [];

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey("SurveyFormId")]
    public virtual SurveyForm SurveyForm { get; set; } = null!;
}

public class SurveyAnswer
{
    public int QuestionId { get; set; }
    public List<int>? SelectedOptionIds { get; set; }
    public string? TextAnswer { get; set; }
}