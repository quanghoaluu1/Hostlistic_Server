using Common;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventService_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Obsolete]
public class SessionBookingsController : ControllerBase
{
    // private readonly ISessionBookingService _sessionBookingService;
    //
    // public SessionBookingsController(ISessionBookingService sessionBookingService)
    // {
    //     _sessionBookingService = sessionBookingService;
    // }
    //
    // [HttpGet("{bookingId:guid}")]
    // public async Task<IActionResult> GetSessionBookingById(Guid bookingId)
    // {
    //     var result = await _sessionBookingService.GetSessionBookingByIdAsync(bookingId);
    //     if (!result.IsSuccess) return BadRequest(result);
    //     return Ok(result);
    // }
    //
    // [HttpGet("session/{sessionId:guid}")]
    // public async Task<IActionResult> GetSessionBookingsBySessionId(Guid sessionId)
    // {
    //     var result = await _sessionBookingService.GetSessionBookingsBySessionIdAsync(sessionId);
    //     if (!result.IsSuccess) return BadRequest(result);
    //     return Ok(result);
    // }
    //
    // [HttpGet("user/{userId:guid}")]
    // public async Task<IActionResult> GetSessionBookingsByUserId(Guid userId)
    // {
    //     var result = await _sessionBookingService.GetSessionBookingsByUserIdAsync(userId);
    //     if (!result.IsSuccess) return BadRequest(result);
    //     return Ok(result);
    // }
    //
    // [HttpPost]
    // public async Task<IActionResult> CreateSessionBooking([FromBody] CreateSessionBookingRequest request)
    // {
    //     var result = await _sessionBookingService.CreateSessionBookingAsync(request);
    //     if (!result.IsSuccess) return BadRequest(result);
    //     return Ok(result);
    // }
    //
    // [HttpPut("{bookingId:guid}")]
    // public async Task<IActionResult> UpdateSessionBooking(Guid bookingId, [FromBody] UpdateSessionBookingRequest request)
    // {
    //     var result = await _sessionBookingService.UpdateSessionBookingAsync(bookingId, request);
    //     if (!result.IsSuccess) return BadRequest(result);
    //     return Ok(result);
    // }
    //
    // [HttpDelete("{bookingId:guid}")]
    // public async Task<IActionResult> DeleteSessionBooking(Guid bookingId)
    // {
    //     var result = await _sessionBookingService.DeleteSessionBookingAsync(bookingId);
    //     if (!result.IsSuccess) return BadRequest(result);
    //     return Ok(result);
    // }
}