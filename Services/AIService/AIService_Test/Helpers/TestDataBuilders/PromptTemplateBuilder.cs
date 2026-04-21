namespace AIService_Test.Helpers.TestDataBuilders;

public class PromptTemplateBuilder
{
    public static PromptTemplate CreatePromptTemplate(
        PromptTemplateKey key = PromptTemplateKey.EventDescription,
        string systemPrompt = "System Prompt",
        string userPrompt = "User Prompt Template with {{event_title}}")
    {
        return new PromptTemplate
        {
            Id = Guid.NewGuid(),
            TemplateKey = key,
            SystemPrompt = systemPrompt,
            UserPromptTemplate = userPrompt,
            DefaultTemperature = 0.7,
            DefaultMaxTokens = 1000,
            CreatedAt = DateTime.UtcNow
        };
    }
}
