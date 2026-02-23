using AIService_Application.DTOs.Requests;

namespace AIService_Application.Prompts;

public class PromptBuilder
{
    public static string BuildSystemPrompt(string language)
    {
        var langInstruction = language is "vi"
            ? "Respond entirely in Vietnamese. Use natural, professional Vietnamese."
            : "Respond entirely in English.";
        return $"""
                You are an expert event copywriter working for a professional event management platform.

                RULES:
                1. {langInstruction}
                2. Return ONLY valid HTML content. No markdown. No code fences. No ```html tags.
                3. Use these HTML tags ONLY: <h2>, <h3>, <p>, <ul>, <ol>, <li>, <strong>, <em>, <br>
                4. Do NOT include <html>, <head>, <body>, or <div> wrapper tags.
                5. The content should be ready to paste directly into a rich text editor.
                6. Structure the description with clear sections.
                7. Keep the total length between 150-400 words.
                8. Do NOT invent specific details (exact numbers, speaker names) that were not provided.
                """;
    }

    public static string BuildDescriptionPrompt(GenerateDescriptionRequest request)
    {
        var toneGuide = request.Tone.ToLower() switch
        {
            "friendly"  => "Use a warm, conversational, and approachable tone. Use inclusive language like 'you' and 'we'.",
            "marketing" => "Use an energetic, persuasive, and action-oriented tone. Include a compelling call-to-action. Emphasize value and urgency.",
            _           => "Use a professional, informative, and authoritative tone. Focus on clarity and credibility."
        };
        var sections = new List<string>
        {
            $"Event Title: {request.EventTitle}",
            $"Event Type: {request.EventType}",
            $"Target Audience: {request.TargetAudience}",
            $"Desired Tone: {toneGuide}"
        };
        if (!string.IsNullOrWhiteSpace(request.Objectives))
            sections.Add($"Objectives: {request.Objectives}");
        if (request.StartDate.HasValue)
            sections.Add($"Start Date: {request.StartDate:MMMM dd, yyyy}");
        if (request.EndDate.HasValue)
            sections.Add($"End Date: {request.EndDate:MMMM dd, yyyy}");
        if (!string.IsNullOrWhiteSpace(request.Location))
            sections.Add($"Location: {request.Location}");
        if (!string.IsNullOrWhiteSpace(request.EventMode))
            sections.Add($"Mode: {request.EventMode}");
        if (request.Keywords?.Count > 0)
            sections.Add($"Keywords to emphasize: {string.Join(", ", request.Keywords)}");
        if (!string.IsNullOrWhiteSpace(request.AdditionalContext))
            sections.Add($"Additional context: {request.AdditionalContext}");

        var eventContext = string.Join("\n", sections);
        return $"""
                Generate a compelling event description based on the following details:

                {eventContext}

                OUTPUT STRUCTURE (use HTML tags):
                1. <h2> — A catchy headline/tagline for the event
                2. <p> — An engaging opening paragraph (2-3 sentences) that hooks the reader
                3. <h3> — "What to Expect" or equivalent section header
                4. <ul> with <li> — 3-5 key highlights or takeaways
                5. <h3> — "Who Should Attend" or equivalent section header
                6. <p> — Brief description of the ideal attendee
                7. <p> — A closing paragraph with a call-to-action

                Generate the description now:
                """;
    }
}