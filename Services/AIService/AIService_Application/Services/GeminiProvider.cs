using AIService_Application.Interface;
using AIService_Domain.Enum;
using Google;
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
    private const int ThinkingTokenBudget = 1024;

    public GeminiProvider(IConfiguration config, ILogger<GeminiProvider> logger)
    {
        var apiKey = config["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini ApiKey is not set");
        _client = new Client(apiKey: apiKey);
        _model = config["Gemini:Model"] ?? "gemini-3-flash-preview";
        _logger = logger;
    }


    public async Task<AiProviderResult> GenerateContentAsync(string systemPrompt, string userPrompt, AiRequestOptions options, CancellationToken cancellationToken = default)
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
                    Temperature = options.Temperature,
                    TopP = 0.9f,
                    MaxOutputTokens = options.MaxTokens,
                    ThinkingConfig = new ThinkingConfig
                    {
                        ThinkingBudget = 0
                    }
                },
                cancellationToken: cancellationToken);
            var text = response.Candidates?[0]?.Content?.Parts?[0].Text;
            var usage = response.UsageMetadata;
            if (response is null || string.IsNullOrEmpty(text))
            {
                throw new Exception("Gemini API returned null response");
            }

            if (response.Candidates is not { Count: > 0 })
            { 
                throw new Exception("Gemini API returned empty candidates");
            }
            var candidate = response.Candidates[0];
            var finishReason = candidate.FinishReason;
            if (finishReason.HasValue)
            {
                var reason = finishReason.Value;

                if (reason == FinishReason.Stop || reason == FinishReason.FinishReasonUnspecified)
                {
                    // ✅ Happy path — content is complete
                }
                else if (reason == FinishReason.MaxTokens)
                {
                    // ⚠️ Truncated but content is still usable
                    _logger.LogWarning(
                        "Gemini response truncated (MAX_TOKENS) for model {Model}. " +
                        "Output may be incomplete. Consider increasing MaxOutputTokens.",
                        _model);
                }
                else if (reason == FinishReason.Safety)
                {
                    throw new InvalidOperationException(
                        "Content blocked by Gemini safety filters. " +
                        "Try adjusting the prompt or tone.");
                }
                else if (reason == FinishReason.Recitation)
                {
                    throw new InvalidOperationException(
                        "Content blocked due to recitation policy. " +
                        "The generated text was too similar to training data.");
                }
                else
                {
                    _logger.LogWarning(
                        "Gemini API returned unexpected finish reason {FinishReason}",
                        reason);
                    throw new Exception(
                        $"Gemini API returned finish reason {reason}");
                }
            }
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
        }catch (GoogleApiException ex)
        {
            // API-level errors: 429 rate limit, 403 quota, 400 bad request
            throw new Exception($"Gemini API error: {ex.Message}", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new Exception("Cannot reach Gemini API", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new Exception("Gemini request timed out", ex);
        }
    }
}