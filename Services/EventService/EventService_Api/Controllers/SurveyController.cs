using System.Security.Claims;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventService_Api.Controllers;

[ApiController]
[Route("api/events/{eventId:guid}/surveys")]
public class SurveyController(ISurveyFormService surveyFormService) : ControllerBase
{
    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    
     // ═══════════════════════════════════════════════
    // ORGANIZER ENDPOINTS
    // ═══════════════════════════════════════════════

    /// <summary>
    /// Create a new survey form for an event.
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateSurvey(Guid eventId, [FromBody] CreateSurveyFormRequest request)
    {
        var result = await surveyFormService.CreateSurveyAsync(eventId, request, GetUserId());
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Update a draft survey form.
    /// </summary>
    [HttpPut("{surveyId:guid}")]
    [Authorize]
    public async Task<IActionResult> UpdateSurvey(Guid eventId, Guid surveyId, [FromBody] UpdateSurveyFormRequest request)
    {
        var result = await surveyFormService.UpdateSurveyAsync(eventId, surveyId, request, GetUserId());
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Delete a draft survey form.
    /// </summary>
    [HttpDelete("{surveyId:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteSurvey(Guid eventId, Guid surveyId)
    {
        var result = await surveyFormService.DeleteSurveyAsync(eventId, surveyId, GetUserId());
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Publish a draft survey (only after event completed).
    /// </summary>
    [HttpPatch("{surveyId:guid}/publish")]
    [Authorize]
    public async Task<IActionResult> PublishSurvey(Guid eventId, Guid surveyId)
    {
        var result = await surveyFormService.PublishSurveyAsync(eventId, surveyId, GetUserId());
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Close a published survey (stop accepting responses).
    /// </summary>
    [HttpPatch("{surveyId:guid}/close")]
    [Authorize]
    public async Task<IActionResult> CloseSurvey(Guid eventId, Guid surveyId)
    {
        var result = await surveyFormService.CloseSurveyAsync(eventId, surveyId, GetUserId());
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get all surveys for an event (organizer view with response counts).
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetSurveys(Guid eventId)
    {
        var result = await surveyFormService.GetSurveysByEventAsync(eventId, GetUserId());
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get survey detail (organizer view).
    /// </summary>
    [HttpGet("{surveyId:guid}")]
    [Authorize]
    public async Task<IActionResult> GetSurveyDetail(Guid eventId, Guid surveyId)
    {
        var result = await surveyFormService.GetSurveyDetailAsync(eventId, surveyId, GetUserId());
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get all individual responses for a survey (organizer only).
    /// </summary>
    [HttpGet("{surveyId:guid}/responses")]
    [Authorize]
    public async Task<IActionResult> GetSurveyResponses(Guid eventId, Guid surveyId)
    {
        var result = await surveyFormService.GetSurveyResponsesAsync(eventId, surveyId, GetUserId());
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get aggregated summary/analytics for a survey (organizer only).
    /// </summary>
    [HttpGet("{surveyId:guid}/summary")]
    [Authorize]
    public async Task<IActionResult> GetSurveySummary(Guid eventId, Guid surveyId)
    {
        var result = await surveyFormService.GetSurveySummaryAsync(eventId, surveyId, GetUserId());
        return StatusCode(result.StatusCode, result);
    }

    // ═══════════════════════════════════════════════
    // ATTENDEE ENDPOINTS
    // ═══════════════════════════════════════════════

    /// <summary>
    /// Get all published surveys for an event (attendee view).
    /// </summary>
    [HttpGet("public")]
    [Authorize]
    public async Task<IActionResult> GetPublicSurveys(Guid eventId)
    {
        var result = await surveyFormService.GetPublicSurveysByEventAsync(eventId, GetUserId());
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Get a single published survey (attendee view).
    /// </summary>
    [HttpGet("{surveyId:guid}/public")]
    [Authorize]
    public async Task<IActionResult> GetPublicSurvey(Guid eventId, Guid surveyId)
    {
        var result = await surveyFormService.GetPublicSurveyAsync(eventId, surveyId, GetUserId());
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Submit survey response (attendee, one-time only).
    /// </summary>
    [HttpPost("{surveyId:guid}/responses")]
    [Authorize]
    public async Task<IActionResult> SubmitResponse(Guid eventId, Guid surveyId, [FromBody] SubmitSurveyResponseRequest request)
    {
        var result = await surveyFormService.SubmitSurveyResponseAsync(eventId, surveyId, request, GetUserId());
        return StatusCode(result.StatusCode, result);
    }
}