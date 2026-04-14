using EventService_Api.Filters;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using EventService_Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventService_Api.Controllers;

[ApiController]
[Route("api/events/{eventId:guid}/ticket-types")]
[Authorize]
public class TicketTypesController(ITicketTypeService ticketTypeService) : ControllerBase
{
    /// <summary>
    /// List all ticket types for the event.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetTicketTypesByEventId(Guid eventId)
    {
        var result = await ticketTypeService.GetTicketTypesByEventIdAsync(eventId);
        return Ok(result);
    }

    /// <summary>
    /// Get a single ticket type by ID.
    /// Also accessible at the legacy path for internal service-to-service calls.
    /// </summary>
    [HttpGet("{ticketTypeId:guid}")]
    [HttpGet("/api/TicketTypes/{ticketTypeId:guid}")] // Legacy: BookingService calls this without eventId
    [AllowAnonymous]
    public async Task<IActionResult> GetTicketTypeById(Guid ticketTypeId, Guid eventId = default)
    {
        var result = await ticketTypeService.GetTicketTypeByIdAsync(ticketTypeId);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// Get ticket types filtered by session.
    /// </summary>
    [HttpGet("session/{sessionId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTicketTypesBySessionId(Guid eventId, Guid sessionId)
    {
        var result = await ticketTypeService.GetTicketTypesBySessionIdAsync(sessionId);
        return Ok(result);
    }

    /// <summary>
    /// Create a new ticket type for the event.
    /// eventId is taken from the route and overrides any value in the request body.
    /// </summary>
    [HttpPost]
    [RequireEventPermission(EventPermissions.ManageTickets)]
    public async Task<IActionResult> CreateTicketType(Guid eventId, [FromBody] CreateTicketTypeRequest request)
    {
        request.EventId = eventId;
        var result = await ticketTypeService.CreateTicketTypeAsync(request);
        if (!result.IsSuccess) return BadRequest(result);
        return CreatedAtAction(
            nameof(GetTicketTypeById),
            new { eventId, ticketTypeId = result.Data?.Id },
            result);
    }

    /// <summary>
    /// Update a ticket type.
    /// </summary>
    [HttpPut("{ticketTypeId:guid}")]
    [RequireEventPermission(EventPermissions.ManageTickets)]
    public async Task<IActionResult> UpdateTicketType(
        Guid eventId, Guid ticketTypeId, [FromBody] UpdateTicketTypeRequest request)
    {
        var result = await ticketTypeService.UpdateTicketTypeAsync(ticketTypeId, request);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// Process a single ticket purchase (called by BookingService / purchase flow).
    /// Also accessible at the legacy path.
    /// </summary>
    [HttpPost("{ticketTypeId:guid}/purchase")]
    [HttpPost("/api/TicketTypes/{ticketTypeId:guid}/purchase")] // Legacy: purchase flow
    [AllowAnonymous]
    public async Task<IActionResult> ProcessTicketPurchase(
        Guid ticketTypeId, [FromBody] UpdateTicketTypeSalesRequest request, Guid eventId = default)
    {
        var result = await ticketTypeService.ProcessTicketPurchaseAsync(ticketTypeId, request.QuantitySold);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Process a bulk ticket purchase (called by BookingService / purchase flow).
    /// Also accessible at the legacy path.
    /// </summary>
    [HttpPost("bulk-purchase")]
    [HttpPost("/api/TicketTypes/bulk-purchase")] // Legacy: purchase flow
    [AllowAnonymous]
    public async Task<IActionResult> ProcessBulkTicketPurchase(
        [FromBody] BulkTicketPurchaseRequest request, Guid eventId = default)
    {
        var result = await ticketTypeService.ProcessBulkTicketPurchaseAsync(request);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Delete a ticket type.
    /// </summary>
    [HttpDelete("{ticketTypeId:guid}")]
    [RequireEventPermission(EventPermissions.ManageTickets)]
    public async Task<IActionResult> DeleteTicketType(Guid eventId, Guid ticketTypeId)
    {
        var result = await ticketTypeService.DeleteTicketTypeAsync(ticketTypeId);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }
}
