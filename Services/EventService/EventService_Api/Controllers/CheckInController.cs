using System.Security.Claims;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventService_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CheckInController : ControllerBase
{
    private readonly ICheckInService _checkInService;

    public CheckInController(ICheckInService checkInService)
    {
        _checkInService = checkInService;
    }

    [HttpGet("{checkInId:guid}")]
    public async Task<IActionResult> GetCheckInById(Guid checkInId)
    {
        var result = await _checkInService.GetCheckInByIdAsync(checkInId);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("event/{eventId:guid}")]
    public async Task<IActionResult> GetCheckInsByEventId(Guid eventId)
    {
        var result = await _checkInService.GetCheckInsByEventIdAsync(eventId);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("session/{sessionId:guid}")]
    public async Task<IActionResult> GetCheckInsBySessionId(Guid sessionId)
    {
        var result = await _checkInService.GetCheckInsBySessionIdAsync(sessionId);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("ticket/{ticketId:guid}")]
    public async Task<IActionResult> GetCheckInByTicketId(Guid ticketId)
    {
        var result = await _checkInService.GetCheckInByTicketIdAsync(ticketId);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateCheckIn([FromBody] CreateCheckInRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _checkInService.CreateCheckInAsync(userId, request);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("{checkInId:guid}")]
    public async Task<IActionResult> UpdateCheckIn(Guid checkInId, [FromBody] UpdateCheckInRequest request)
    {
        var result = await _checkInService.UpdateCheckInAsync(checkInId, request);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{checkInId:guid}")]
    public async Task<IActionResult> DeleteCheckIn(Guid checkInId)
    {
        var result = await _checkInService.DeleteCheckInAsync(checkInId);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException("User ID not found in token"));
    }
}
