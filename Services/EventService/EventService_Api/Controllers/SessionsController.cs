using Common;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventService_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Obsolete]
//[Authorize]
public class SessionsController : ControllerBase
{
    // private readonly ISessionService _sessionService;
    //
    // public SessionsController(ISessionService sessionService)
    // {
    //     _sessionService = sessionService;
    // }
    //
    // [HttpGet("{sessionId:guid}")]
    // public async Task<IActionResult> GetSessionById(Guid sessionId)
    // {
    //     var result = await _sessionService.GetSessionByIdAsync(TODO, sessionId);
    //     if (!result.IsSuccess) return BadRequest(result);
    //     return Ok(result);
    // }
    //
    // [HttpGet("event/{eventId:guid}")]
    // public async Task<IActionResult> GetSessionsByEventId(Guid eventId)
    // {
    //     var result = await _sessionService.GetSessionsByEventIdAsync(eventId);
    //     if (!result.IsSuccess) return BadRequest(result);
    //     return Ok(result);
    // }
    //
    // [HttpGet("track/{trackId:guid}")]
    // public async Task<IActionResult> GetSessionsByTrackId(Guid trackId)
    // {
    //     var result = await _sessionService.GetSessionsByTrackIdAsync(trackId);
    //     if (!result.IsSuccess) return BadRequest(result);
    //     return Ok(result);
    // }
    //
    // [HttpGet("venue/{venueId:guid}")]
    // public async Task<IActionResult> GetSessionsByVenueId(Guid venueId)
    // {
    //     var result = await _sessionService.GetSessionsByVenueIdAsync(venueId);
    //     if (!result.IsSuccess) return BadRequest(result);
    //     return Ok(result);
    // }
    //
    // [HttpPost]
    // public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest request)
    // {
    //     var result = await _sessionService.CreateSessionAsync(request);
    //     if (!result.IsSuccess) return BadRequest(result);
    //     return Ok(result);
    // }
    //
    // [HttpPut("{sessionId:guid}")]
    // public async Task<IActionResult> UpdateSession(Guid sessionId, [FromBody] UpdateSessionRequest request)
    // {
    //     var result = await _sessionService.UpdateSessionAsync(sessionId, request);
    //     if (!result.IsSuccess) return BadRequest(result);
    //     return Ok(result);
    // }
    //
    // [HttpDelete("{sessionId:guid}")]
    // public async Task<IActionResult> DeleteSession(Guid sessionId)
    // {
    //     var result = await _sessionService.DeleteSessionAsync(sessionId);
    //     if (!result.IsSuccess) return BadRequest(result);
    //     return Ok(result);
    // }
}