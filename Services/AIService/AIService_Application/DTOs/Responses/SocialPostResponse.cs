namespace AIService_Application.DTOs.Responses;

public record SocialPostResponse
{
    public Guid ContentId { get; init; }
    public Guid RequestId { get; init; }
    
    public string PostContent { get; init; } = string.Empty;
    
    public string Hashtags { get; init; } = string.Empty;
    
    public string Platform { get; init; } = string.Empty;
    public string Length { get; init; } = string.Empty;
    public string Tone { get; init; } = string.Empty;
    public string Language { get; init; } = string.Empty;
    
    public int CharacterCount { get; init; }
    
    public bool ExceedsLimit { get; init; }
    
    public string Model { get; init; } = string.Empty;
    public int TokensUsed { get; init; }
    public long GenerationTimeMs { get; init; }
}