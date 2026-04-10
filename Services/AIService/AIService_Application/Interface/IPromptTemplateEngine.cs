using AIService_Application.DTOs.Requests;
using AIService_Application.DTOs.Responses;

namespace AIService_Domain.Interfaces;

public interface IPromptTemplateEngine
{
    string Render(string template, Dictionary<string, string> parameters);
    Dictionary<string, string> BuildParametersFromEvent(EventDetailDto eventDetail);
    Dictionary<string, string> BuildEmailParameters(EventDetailDto eventEntity, GenerateEmailRequest request);

    Dictionary<string, string> BuildSocialPostParameters(EventDetailDto eventEntity,
        GenerateSocialPostRequest request);
    Dictionary<string, string> BuildSpeakerIntroParameters(
        EventDetailDto eventDetail,
        LineupTalentDto talent,             
        List<string> talentSessionNames,   
        bool isEventWide,                   
        GenerateSpeakerIntroRequest request);

    Dictionary<string, string> BuildSessionAbstractParameters(
        EventDetailDto eventDetail,
        SessionDetailDto session,
        TrackDetailDto track,
        GenerateSessionAbstractRequest request);
    Dictionary<string, string> AddToneAndLanguage(
        Dictionary<string, string> parameters,
        string tone,
        string language);

    (string subject, string htmlBody) ParseEmailResponse(string rawContent);

    (string content, string hashtags) ParseSocialPostResponse(
        string rawContent, string? requestedHashtags);
    string SanitizeHtml(string html);
    int GetPlatformCharacterLimit(string platform);
}