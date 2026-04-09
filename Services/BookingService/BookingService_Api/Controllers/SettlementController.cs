using System.Security.Claims;
using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingService_Api.Controllers;

[ApiController]
[Route("api/admin/settlements")]
[Authorize(Roles = "Admin")]
public class SettlementController(ISettlementService settlementService) : ControllerBase
{
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingSettlements(CancellationToken ct)
    {
        var result = await settlementService.GetPendingSettlementsAsync(ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllSettlements(CancellationToken ct)
    {
        var result = await settlementService.GetAllSettlementsAsync(ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{eventId:guid}/preview")]
    public async Task<IActionResult> PreviewSettlement(Guid eventId, Guid organizerId, CancellationToken ct)
    {
        var result = await settlementService.PreviewSettlementAsync(eventId, organizerId, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{eventId:guid}/settle")]
    public async Task<IActionResult> SettleEvent(
        Guid eventId,
        [FromBody] SettleEventRequest request,
        CancellationToken ct)
    {
        var adminId = GetUserId();
        var result = await settlementService.SettleEventAsync(eventId, request.OrganizerId, adminId, request.AdminNotes, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{eventId:guid}/reject")]
    public async Task<IActionResult> RejectSettlement(
        Guid eventId,
        [FromBody] RejectSettlementRequest request,
        CancellationToken ct)
    {
        var adminId = GetUserId();
        var result = await settlementService.RejectSettlementAsync(eventId, adminId, request.Reason, ct);
        return StatusCode(result.StatusCode, result);
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? throw new UnauthorizedAccessException("User ID not found in claims."));
}
