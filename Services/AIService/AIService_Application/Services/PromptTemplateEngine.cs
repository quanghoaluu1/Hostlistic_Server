using System.Text.RegularExpressions;
using AIService_Application.DTOs.Responses;
using AIService_Domain.Interfaces;

namespace AIService_Application.Services;

public partial class PromptTemplateEngine : IPromptTemplateEngine
{
     /// <summary>
    /// Render template: xử lý {{#if}}, {{#each}}, {{placeholder}}
    /// </summary>
    public string Render(string template, Dictionary<string, string> parameters)
    {
        var result = template;

        // 1. {{#if key}}...{{else}}...{{/if}}
        result = IfElseRegex().Replace(result, match =>
        {
            var key = match.Groups[1].Value;
            var ifBlock = match.Groups[2].Value;
            var elseBlock = match.Groups[3].Value;

            return HasValue(parameters, key)
                ? Render(ifBlock, parameters)
                : Render(elseBlock, parameters);
        });

        // 2. {{#if key}}...{{/if}} (không có else)
        result = IfRegex().Replace(result, match =>
        {
            var key = match.Groups[1].Value;
            var content = match.Groups[2].Value;

            return HasValue(parameters, key)
                ? Render(content, parameters)
                : string.Empty;
        });

        // 3. {{placeholder}} → giá trị thực
        result = PlaceholderRegex().Replace(result, match =>
        {
            var key = match.Groups[1].Value;
            return parameters.TryGetValue(key, out var value)
                ? value
                : string.Empty; // Không để lại [key], giữ prompt sạch
        });

        // 4. Clean up: xóa dòng trống thừa (>2 liên tiếp → 2)
        result = ExcessiveNewlinesRegex().Replace(result, "\n\n");

        return result.Trim();
    }

    /// <summary>
    /// Build parameters từ EventDetailDto — reuse cho mọi content type
    /// </summary>
    public Dictionary<string, string> BuildParametersFromEvent(EventDetailDto eventDetail)
    {
        var parameters = new Dictionary<string, string>
        {
            ["event_title"] = eventDetail.Title,
            ["event_type"] = eventDetail.EventTypeName,
            ["event_date"] = eventDetail.StartDate.Value.ToString("MMMM dd, yyyy"),
            ["event_time"] = eventDetail.StartDate.Value.ToString("hh:mm tt"),
            ["event_end_date"] = eventDetail.EndDate.Value.ToString("MMMM dd, yyyy"),
            ["event_location"] = eventDetail.Location ?? "TBD",
            ["event_mode"] = eventDetail.EventMode.ToString(),
            ["registration_link"] = $"https://hostlistic.com/events/{eventDetail.Id}",
            ["total_capacity"] = eventDetail.TotalCapacity.ToString(),
        };

        // Venue
        if (eventDetail.Venue is not null)
        {
            parameters["venue_name"] = eventDetail.Venue.Name;
            parameters["venue_address"] = eventDetail.Venue.Address;
        }

        // Tracks + Sessions → key_topics

            var topics = eventDetail.Tracks
                .Select(t =>
                    $"{t.Name}: {string.Join(", ", t.Sessions.Select(s => s.Title))}")
                .Take(5);

            parameters["key_topics"] = string.Join(" | ", topics);
            
        // Speakers/Talents (flatten từ tất cả sessions)
        var talents = eventDetail.Tracks
            .SelectMany(t => t.Sessions)
            .SelectMany(s => s.Talents)
            .DistinctBy(t => t.Id)
            .ToList();

        if (talents.Count > 0)
        {
            parameters["speakers"] = string.Join(", ",
                talents.Select(t => string.IsNullOrEmpty(t.Type)
                    ? t.Name
                    : $"{t.Name} ({t.Type})"));

            parameters["speaker_count"] = talents.Count.ToString();
        }

        // Session count
        var sessionCount = eventDetail.Tracks
            .SelectMany(t => t.Sessions)
            .Count();

        if (sessionCount > 0)
            parameters["session_count"] = sessionCount.ToString();

        return parameters;
    }

    public Dictionary<string, string> AddToneAndLanguage(
        Dictionary<string, string> parameters,
        string tone,
        string language)
    {
        // Tone guide — chi tiết hơn để AI output đúng style
        parameters["tone_guide"] = tone.ToLower() switch
        {
            "friendly" => "Warm, conversational, and approachable. Use 'you' and 'we'. " +
                          "Feel like a friend inviting you to something exciting.",
            "marketing" => "Energetic, persuasive, and action-oriented. Emphasize value, " +
                           "urgency, and FOMO. Include compelling call-to-action.",
            _ => "Professional, informative, and authoritative. " +
                 "Focus on clarity, credibility, and structured information."
        };

        // Language instruction
        parameters["language_instruction"] = language.ToLower() switch
        {
            "vi" or "vietnamese" =>
                "Respond entirely in Vietnamese. Use natural, professional Vietnamese.",
            _ =>
                "Respond entirely in English."
        };

        return parameters;
    }
    
    // ─── Helpers ─────────────────────────────────────

    private static bool HasValue(Dictionary<string, string> parameters, string key)
        => parameters.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value);

    // ─── Source-generated Regex (.NET 7+) ────────────

    [GeneratedRegex(@"\{\{#if\s+(\w+)\}\}(.*?)\{\{else\}\}(.*?)\{\{/if\}\}", RegexOptions.Singleline)]
    private static partial Regex IfElseRegex();

    [GeneratedRegex(@"\{\{#if\s+(\w+)\}\}(.*?)\{\{/if\}\}", RegexOptions.Singleline)]
    private static partial Regex IfRegex();

    [GeneratedRegex(@"\{\{(\w+)\}\}")]
    private static partial Regex PlaceholderRegex();

    [GeneratedRegex(@"\n{3,}")]
    private static partial Regex ExcessiveNewlinesRegex();
    
}