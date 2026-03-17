using System.Security.Claims;
using AIService_Application.DTOs.Requests;
using AIService_Application.DTOs.Responses;
using AIService_Application.Interface;
using Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIService_Api.Controllers;

[ApiController]
[Route("api/ai")]
[Produces("application/json")]
public class AiContentController(IAiContentService aiContentService, ILogger<AiContentController> logger) : ControllerBase
{
    [HttpPost("generate-description")]
    [ProducesResponseType(typeof(AiContentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AiContentResponse>> GenerateDescription(
        [FromBody] GenerateDescriptionRequest request,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException("User ID not found in token"));

        try
        {
            var result = await aiContentService
                .GenerateDescriptionAsync(request, userId, ct);
            return Ok(result);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("429"))
        {
            logger.LogWarning("Gemini rate limit hit for event {EventId}", request.EventId);
            return StatusCode(429, new
            {
                error = "AI service rate limit exceeded. Please retry in a few seconds."
            });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Empty response"))
        {
            logger.LogWarning("Gemini returned empty response for event {EventId}", request.EventId);
            return StatusCode(502, new
            {
                error = "AI service returned an empty response. Please try again."
            });
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499, new { error = "Request cancelled by client." });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error generating description for event {EventId}",
                request.EventId);
            return StatusCode(500, new
            {
                error = "An unexpected error occurred while generating content."
            });
        }
    }
    
    [HttpPost("generate-email")]
    [ProducesResponseType(typeof(EmailContentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GenerateEmail(
        [FromBody] GenerateEmailRequest request,
        CancellationToken ct)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException("User ID not found in token"));
        var result = await aiContentService.GenerateEmailAsync(request, userId, ct);

        return Ok(result);
    }
    
    [HttpPost("generate-social-post")]
    [ProducesResponseType(typeof(ApiResponse<SocialPostResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GenerateSocialPost(
        [FromBody] GenerateSocialPostRequest request,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = Guid.Parse(userIdClaim ??
                                throw new UnauthorizedAccessException("User ID not found in token"));

        try
        {
            var result = await aiContentService.GenerateSocialPostAsync(request, userId, ct);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("429"))
        {
            logger.LogWarning("Gemini rate limit hit for social post, event {EventId}",
                request.EventId);
            return StatusCode(429, new
            {
                error = "AI service rate limit exceeded. Please retry in a few seconds."
            });
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499, new { error = "Request cancelled by client." });
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Unexpected error generating social post for event {EventId}",
                request.EventId);
            return StatusCode(500, new
            {
                error = "An unexpected error occurred while generating content."
            });
        }
    }
}