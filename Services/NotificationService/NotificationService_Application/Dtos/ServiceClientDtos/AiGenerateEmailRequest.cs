namespace NotificationService_Application.Dtos.ServiceClientDtos;

public sealed record AiGenerateEmailRequest
{
    public Guid EventId { get; init; }
    public string EmailType { get; init; } = string.Empty;
    public string Tone { get; init; } = "formal";
    public string Language { get; init; } = "English";
    public string? AgendaHighlights { get; init; }
    public string? CheckinInstructions { get; init; }
    public string? PreparationNotes { get; init; }
}

public sealed record AiEmailContentDto
{
    public string SubjectLine { get; init; } = string.Empty;
    public string HtmlBody { get; init; } = string.Empty;
}