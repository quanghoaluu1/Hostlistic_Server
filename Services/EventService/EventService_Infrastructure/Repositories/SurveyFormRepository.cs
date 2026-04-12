using EventService_Domain.Entities;
using EventService_Domain.Interfaces;
using EventService_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventService_Infrastructure.Repositories;

public class SurveyFormRepository(EventServiceDbContext context): ISurveyFormRepository
{
     public async Task<SurveyForm?> GetByIdAsync(Guid surveyFormId)
    {
        return await context.SurveyForms
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == surveyFormId);
    }

    public async Task<SurveyForm?> GetByIdWithResponsesAsync(Guid surveyFormId)
    {
        return await context.SurveyForms
            .Include(s => s.Responses)
            .FirstOrDefaultAsync(s => s.Id == surveyFormId);
    }

    public async Task<List<SurveyForm>> GetByEventIdAsync(Guid eventId)
    {
        return await context.SurveyForms
            .AsNoTracking()
            .Where(s => s.EventId == eventId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(SurveyForm surveyForm)
    {
        await context.SurveyForms.AddAsync(surveyForm);
        await context.SaveChangesAsync();
    }

    public void Update(SurveyForm surveyForm)
    {
        context.SurveyForms.Update(surveyForm);
    }

    public void Delete(SurveyForm surveyForm)
    {
        context.SurveyForms.Remove(surveyForm);
    }

    public async Task<SurveyResponse?> GetResponseAsync(Guid surveyFormId, Guid userId)
    {
        return await context.SurveyResponses
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.SurveyFormId == surveyFormId && r.UserId == userId);
    }

    public async Task<List<SurveyResponse>> GetResponsesBySurveyIdAsync(Guid surveyFormId)
    {
        return await context.SurveyResponses
            .AsNoTracking()
            .Where(r => r.SurveyFormId == surveyFormId)
            .OrderByDescending(r => r.SubmittedAt)
            .ToListAsync();
    }

    public async Task<int> GetResponseCountAsync(Guid surveyFormId)
    {
        return await context.SurveyResponses
            .CountAsync(r => r.SurveyFormId == surveyFormId);
    }

    public async Task AddResponseAsync(SurveyResponse response)
    {
        await context.SurveyResponses.AddAsync(response);
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
}