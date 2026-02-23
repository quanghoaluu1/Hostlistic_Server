using AIService_Application.Interface;
using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AIService_Application.Services;

public class GeminiProvider : IAiProvider
{
    private readonly Client _client;
    private readonly string _model;
    private readonly ILogger<GeminiProvider> _logger;

    public GeminiProvider(IConfiguration config, ILogger<GeminiProvider> logger)
    {
        var apiKey = config["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini ApiKey is not set");
        _client = new Client(apiKey: apiKey);
        _model = config["Gemini:Model"] ?? "gemini-3-flash-preview";
        _logger = logger;
    }


    public async Task<AiProviderResult> GenerateContentAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Calling Gemini API with model {Model}", _model);
        try
        {
            var response = await _client.Models.GenerateContentAsync(
                model: _model,
                contents: userPrompt,
                config: new GenerateContentConfig
                {
                    SystemInstruction = new Content
                    {
                        Parts = [new Part { Text = systemPrompt }]
                    },
                    Temperature = 0.7f,
                    TopP = 0.9f,
                    MaxOutputTokens = 2048
                },
                cancellationToken: cancellationToken);
            var text = response.Candidates?[0]?.Content?.Parts?[0].Text;
            var usage = response.UsageMetadata;
            return new AiProviderResult(
                Content: text,
                PromptTokens: usage?.PromptTokenCount ?? 0,
                CompletionTokens: usage?.CandidatesTokenCount ?? 0,
                Model: _model);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error calling Gemini API");
            throw;
        }
    }
}