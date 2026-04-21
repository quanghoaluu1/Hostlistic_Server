namespace AIService_Test.Helpers.TestDataBuilders;

public class AiRequestBuilder
{
    public static GenerateDescriptionRequest CreateDescriptionRequest(
        Guid? eventId = null,
        string tone = "professional",
        string language = "en")
    {
        return new GenerateDescriptionRequest
        {
            EventId = eventId ?? Guid.NewGuid(),
            Tone = tone,
            Language = language,
            Keywords = new List<string> { "AI", "Test" }
        };
    }

    public static GenerateEmailRequest CreateEmailRequest(
        Guid? eventId = null,
        string emailType = "invitation",
        string tone = "warm",
        string language = "vi")
    {
        return new GenerateEmailRequest
        {
            EventId = eventId ?? Guid.NewGuid(),
            EmailType = emailType,
            Tone = tone,
            Language = language
        };
    }

    public static GenerateSocialPostRequest CreateSocialPostRequest(
        Guid? eventId = null,
        string platform = "twitter",
        string language = "en")
    {
        return new GenerateSocialPostRequest
        {
            EventId = eventId ?? Guid.NewGuid(),
            Platform = platform,
            Tone = "exciting",
            Language = language,
            Length = "short"
        };
    }

    public static GenerateSpeakerIntroRequest CreateSpeakerIntroRequest(
        Guid? eventId = null,
        Guid? talentId = null,
        string mode = "from_name")
    {
        return new GenerateSpeakerIntroRequest
        {
            EventId = eventId ?? Guid.NewGuid(),
            TalentId = talentId ?? Guid.NewGuid(),
            Mode = mode,
            Tone = "professional",
            Language = "en"
        };
    }

    public static GenerateSessionAbstractRequest CreateSessionAbstractRequest(
        Guid? eventId = null,
        Guid? sessionId = null,
        string mode = "from_metadata")
    {
        return new GenerateSessionAbstractRequest
        {
            EventId = eventId ?? Guid.NewGuid(),
            SessionId = sessionId ?? Guid.NewGuid(),
            Mode = mode,
            Tone = "academic",
            Language = "en"
        };
    }
}
