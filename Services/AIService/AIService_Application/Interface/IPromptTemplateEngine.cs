using AIService_Application.DTOs.Responses;

namespace AIService_Domain.Interfaces;

public interface IPromptTemplateEngine
{
    string Render(string template, Dictionary<string, string> parameters);
    Dictionary<string, string> BuildParametersFromEvent(EventDetailDto eventDetail);

    Dictionary<string, string> AddToneAndLanguage(
        Dictionary<string, string> parameters,
        string tone,
        string language);
}