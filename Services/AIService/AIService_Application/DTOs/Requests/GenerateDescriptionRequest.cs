using System.ComponentModel.DataAnnotations;

namespace AIService_Application.DTOs.Requests;

public class GenerateDescriptionRequest : AiRequestBase
{
    [Required]
    public string EventTitle { get; set; } = string.Empty;
    [Required]
    public string EventType { get; set; } = string.Empty;
    public string? TargetAudience { get; set; }
    public string? Objectives { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Location { get; set; }
    public string? EventMode { get; set; }
    public List<string>? Keywords { get; set; }
}