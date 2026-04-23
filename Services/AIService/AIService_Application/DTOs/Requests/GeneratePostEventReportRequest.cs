using System.ComponentModel.DataAnnotations;

namespace AIService_Application.DTOs.Requests;

public record GeneratePostEventReportRequest
{
    [Required]
    public Guid EventId { get; init; }

    /// <summary>Output language for the generated report. Defaults to English.</summary>
    [Required]
    public string Language { get; init; } = "English";
}
