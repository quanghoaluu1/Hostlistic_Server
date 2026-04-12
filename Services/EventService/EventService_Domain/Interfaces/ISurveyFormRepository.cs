using EventService_Domain.Entities;

namespace EventService_Domain.Interfaces;

public interface ISurveyFormRepository
{
    Task<SurveyForm?> GetByIdAsync(Guid surveyFormId);
    Task<SurveyForm?> GetByIdWithResponsesAsync(Guid surveyFormId);
    Task<List<SurveyForm>> GetByEventIdAsync(Guid eventId);
    Task AddAsync(SurveyForm surveyForm);
    void Update(SurveyForm surveyForm);
    void Delete(SurveyForm surveyForm);

    // SurveyResponse
    Task<SurveyResponse?> GetResponseAsync(Guid surveyFormId, Guid userId);
    Task<List<SurveyResponse>> GetResponsesBySurveyIdAsync(Guid surveyFormId);
    Task<int> GetResponseCountAsync(Guid surveyFormId);
    Task AddResponseAsync(SurveyResponse response);

    Task SaveChangesAsync();
}