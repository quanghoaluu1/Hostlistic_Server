using System.Security.Claims;
using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingService_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpGet("{ticketId:guid}")]
    public async Task<IActionResult> GetTicketById(Guid ticketId)
    {
        var result = await _ticketService.GetTicketByIdAsync(ticketId);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("code/{ticketCode}")]
    public async Task<IActionResult> GetTicketByCode(string ticketCode)
    {
        var result = await _ticketService.GetTicketByCodeAsync(ticketCode);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("events/{eventId:guid}/stream-access/{userId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckStreamAccess(Guid eventId, Guid userId)
    {
        var result = await _ticketService.CheckStreamAccessAsync(eventId, userId);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result);
        return Ok(result);
    }

    [HttpPost("guest-live-access")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateGuestLiveAccess([FromBody] ValidateGuestLiveAccessRequest request)
    {
        var result = await _ticketService.ValidateGuestLiveAccessAsync(request);
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result);
        return Ok(result);
    }

    [HttpGet("order/{orderId:guid}")]
    public async Task<IActionResult> GetTicketsByOrderId(Guid orderId)
    {
        var result = await _ticketService.GetTicketsByOrderIdAsync(orderId);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTicket([FromBody] CreateTicketRequest request)
    {
        var result = await _ticketService.CreateTicketAsync(request);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("{ticketId:guid}")]
    public async Task<IActionResult> UpdateTicket(Guid ticketId, [FromBody] UpdateTicketRequest request)
    {
        var result = await _ticketService.UpdateTicketAsync(ticketId, request);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("admin/regenerate-qr-codes")]
    public async Task<IActionResult> RegenerateQrCodes()
    {
        var result = await _ticketService.RegenerateAllQrCodesAsync();
        return Ok(result);
    }

    [HttpDelete("{ticketId:guid}")]
    public async Task<IActionResult> DeleteTicket(Guid ticketId)
    {
        var result = await _ticketService.DeleteTicketAsync(ticketId);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("{ticketId:guid}/postponement-decision")]
    public async Task<IActionResult> PostponementDecision(Guid ticketId, [FromBody] PostponementDecisionRequest request)
    {
        var callerUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found in token."));

        var result = await _ticketService.ProcessPostponementDecisionAsync(ticketId, request.Decision, callerUserId);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("admin/events/{eventId:guid}/process-refunds")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ProcessRefundsForPostponedEvent(Guid eventId)
    {
        var result = await _ticketService.ProcessRefundsForPostponedEventAsync(eventId);
        return StatusCode(result.StatusCode, result);
    }
}
