namespace AIService_Application.Interface;

public interface IAiProvider
{
    Task<AiProviderResult> GenerateContentAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default);
}
public record AiProviderResult(
    string Content,
    int PromptTokens,
    int CompletionTokens,
    string Model);