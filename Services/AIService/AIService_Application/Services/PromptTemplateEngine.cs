using System.Text.RegularExpressions;
using AIService_Application.DTOs.Requests;
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
    
     public Dictionary<string, string> BuildEmailParameters(EventDetailDto eventEntity, GenerateEmailRequest request)
    {
        var parameters = new Dictionary<string, string>()
        {
            // Auto-fill từ Event entity
            ["event_title"] = eventEntity.Title,
            ["event_type"] = eventEntity.EventTypeName.ToString(),
            ["event_date"] = eventEntity.StartDate.Value.ToString("MMMM dd, yyyy"),
            ["event_time"] = eventEntity.StartDate.Value.ToString("hh:mm tt"),
            ["event_location"] = eventEntity.Location ?? "TBD",
            ["event_mode"] = eventEntity.EventMode,
            ["registration_link"] = $"https://hostlistic.tech/events/{eventEntity.Id}/register",

            // Từ request
            ["tone"] = request.Tone,
            ["language"] = request.Language,
            ["recipient_type"] = request.RecipientType ?? "general attendees",
            ["target_audience"] = request.TargetAudience ?? "",
            ["selling_points"] = request.SellingPoints ?? "",
            ["reminder_type"] = request.EmailType.Replace("reminder_", ""),
        };
        var talents = eventEntity.Tracks.SelectMany(t => t.Sessions).SelectMany(t => t.Talents).Select(s => $"{s.Name} ({s.Type}").Distinct();
        parameters["talents"] = talents?.Any() is true ? string.Join(", ", talents) : "TBA";
        var topics = eventEntity.Tracks
            .Select(t => $"{t.Name}: {string.Join(", ", t.Sessions.Select(s => s.Title))}")
            .Take(5);

        parameters["key_topics"] = topics.Any()
            ? string.Join(" | ", topics)
            : "";
        
        // Optional fields
        if (!string.IsNullOrEmpty(request.EarlyBirdDeadline))
            parameters["early_bird_deadline"] = request.EarlyBirdDeadline;
        if (!string.IsNullOrEmpty(request.EarlyBirdDiscount))
            parameters["early_bird_discount"] = request.EarlyBirdDiscount;
        if (!string.IsNullOrEmpty(request.TicketPrice))
            parameters["ticket_price"] = request.TicketPrice;
        if (!string.IsNullOrEmpty(request.CheckinInstructions))
            parameters["checkin_instructions"] = request.CheckinInstructions;
        if (!string.IsNullOrEmpty(request.PreparationNotes))
            parameters["preparation_notes"] = request.PreparationNotes;
        if (!string.IsNullOrEmpty(request.AgendaHighlights))
            parameters["agenda_highlights"] = request.AgendaHighlights;
        if (!string.IsNullOrEmpty(request.AttendeeName))
            parameters["attendee_name"] = request.AttendeeName;
        if (!string.IsNullOrEmpty(request.TicketType))
            parameters["ticket_type"] = request.TicketType;

        return parameters;
    }


    public Dictionary<string, string> BuildSocialPostParameters(EventDetailDto eventEntity,
        GenerateSocialPostRequest request)
    {
        var parameters = new Dictionary<string, string>
        {
            // Event context (auto-enriched)
            ["event_title"] = eventEntity.Title,
            ["event_type"] = eventEntity.EventTypeName,
            ["event_date"] = eventEntity.StartDate?.ToString("MMMM dd, yyyy") ?? "TBD",
            ["event_time"] = eventEntity.StartDate?.ToString("hh:mm tt") ?? "",
            ["event_location"] = eventEntity.Location ?? "Online",
            ["event_mode"] = eventEntity.EventMode ?? "Offline",
            ["registration_link"] = $"https://hostlistic.tech/events/{eventEntity.Id}",

            // Request-specific
            ["platform"] = request.Platform,
            ["length"] = request.Length,
            ["tone"] = request.Tone,
            ["language"] = request.Language,
            ["character_limit"] = GetPlatformCharacterLimit(request.Platform).ToString(),
        };

        // Speakers
        var talents = eventEntity.Tracks
            .SelectMany(t => t.Sessions)
            .SelectMany(s => s.Talents)
            .Select(t => t.Name)
            .Distinct()
            .ToList();
        parameters["speakers"] = talents.Count > 0
            ? string.Join(", ", talents)
            : "";

        // Topics from tracks/sessions
        var topics = eventEntity.Tracks
            .SelectMany(t => t.Sessions.Select(s => s.Title))
            .Distinct()
            .Take(5)
            .ToList();
        parameters["key_topics"] = topics.Count > 0
            ? string.Join(", ", topics)
            : "";

        // Optional fields
        if (!string.IsNullOrWhiteSpace(request.Hashtags))
            parameters["hashtags"] = request.Hashtags;
        if (!string.IsNullOrWhiteSpace(request.CallToAction))
            parameters["call_to_action"] = request.CallToAction;
        if (!string.IsNullOrWhiteSpace(request.KeyHighlights))
            parameters["key_highlights"] = request.KeyHighlights;
        var lengthKey = $"length_{request.Length.ToLowerInvariant()}";
        parameters[lengthKey] = "true";
        return parameters;
    }
    public (string subject, string htmlBody) ParseEmailResponse(string rawContent)
    {
        var sanitized = SanitizeHtml(rawContent);
        var subjectMatch = MyRegex().Match(sanitized);
        var subject = subjectMatch.Success ? subjectMatch.Groups[1].Value.Trim() : "You're Invited!";
        var body = sanitized;
        if (subjectMatch.Success)
        {
            body = sanitized.Substring(subjectMatch.Index + subjectMatch.Length).Trim();
        }

        if (!body.TrimStart().StartsWith("<div>", StringComparison.OrdinalIgnoreCase))
        {
            body = $"<div>{body}</div>";
        }
        return (subject, body);
    }

    
    public (string content, string hashtags) ParseSocialPostResponse(
    string rawContent, string? requestedHashtags)
{
    var cleaned = rawContent.Trim();

    // Remove Markdown code fences if present
    cleaned = Regex.Replace(cleaned, @"^```\w*\s*\n?", "", RegexOptions.Multiline);
    cleaned = Regex.Replace(cleaned, @"\n?```\s*$", "", RegexOptions.Multiline);
    cleaned = cleaned.Trim();

    // Split content from hashtag lines
    // Hashtags pattern: line starting with # or containing multiple #words
    var lines = cleaned.Split('\n');
    var contentLines = new List<string>();
    var hashtagLines = new List<string>();
    var hitHashtagZone = false;

    for (var i = lines.Length - 1; i >= 0; i--)
    {
        var line = lines[i].Trim();
        if (string.IsNullOrEmpty(line))
        {
            if (hitHashtagZone) continue; // skip blank between content and hashtags
            contentLines.Insert(0, line);
            continue;
        }

        // A line is "hashtag-only" if >50% of words start with #
        var words = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var hashtagWordCount = words.Count(w => w.StartsWith('#'));
        var isHashtagLine = words.Length > 0 &&
                            (double)hashtagWordCount / words.Length > 0.5;

        if (isHashtagLine && !hitHashtagZone)
        {
            hitHashtagZone = true;
            hashtagLines.Insert(0, line);
        }
        else
        {
            contentLines.Insert(0, line);
        }
    }

    var content = string.Join('\n', contentLines).Trim();
    var parsedHashtags = string.Join(' ', hashtagLines).Trim();

    // Merge with user-requested hashtags (deduplicate)
    if (!string.IsNullOrWhiteSpace(requestedHashtags))
    {
        var existing = parsedHashtags
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(h => h.TrimStart('#').ToLowerInvariant())
            .ToHashSet();

        var userTags = requestedHashtags
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(h => !existing.Contains(h.TrimStart('#').ToLowerInvariant()));

        var merged = string.IsNullOrEmpty(parsedHashtags)
            ? string.Join(' ', userTags)
            : $"{parsedHashtags} {string.Join(' ', userTags)}";

        parsedHashtags = merged.Trim();
    }

    return (content, parsedHashtags);
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
    
    public int GetPlatformCharacterLimit(string platform) => platform.ToLowerInvariant() switch
    {
        "twitter" => 280,
        "linkedin" => 3000,
        "instagram" => 2200,
        "facebook" => 0,       // FB has ~63,206 char limit — practically unlimited
        _ => 0
    };
    
    public string SanitizeHtml(string raw)
    {
        var cleaned = Regex.Replace(raw, @"^```html?\s*\n?", "", RegexOptions.Multiline);
        cleaned = Regex.Replace(cleaned, @"\n?```\s*$", "", RegexOptions.Multiline);
        cleaned = Regex.Replace(cleaned, @"<\/?(html|head|body|div)[^>]*>", "",
            RegexOptions.IgnoreCase);
        cleaned = Regex.Replace(cleaned, @">\s+<", "><");
        return cleaned.Trim();
    }
    

    // ─── Source-generated Regex (.NET 7+) ────────────
    [GeneratedRegex(@"SUBJECT:\s*(.+?)(?:\n|<br|<\/?\w)", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex MyRegex();
    [GeneratedRegex(@"\{\{#if\s+(\w+)\}\}(.*?)\{\{else\}\}(.*?)\{\{/if\}\}", RegexOptions.Singleline)]
    private static partial Regex IfElseRegex();

    [GeneratedRegex(@"\{\{#if\s+(\w+)\}\}(.*?)\{\{/if\}\}", RegexOptions.Singleline)]
    private static partial Regex IfRegex();

    [GeneratedRegex(@"\{\{(\w+)\}\}")]
    private static partial Regex PlaceholderRegex();

    [GeneratedRegex(@"\n{3,}")]
    private static partial Regex ExcessiveNewlinesRegex();
    
}