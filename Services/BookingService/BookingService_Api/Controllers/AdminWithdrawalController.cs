using System.Security.Claims;
using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using BookingService_Domain.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingService_Api.Controllers;

[ApiController]
[Route("api/admin/withdrawals")]
[Authorize(Roles = "Admin")]
public class AdminWithdrawalController(IWithdrawalRequestService withdrawalService) : ControllerBase
{
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingWithdrawals(CancellationToken ct)
    {
        var result = await withdrawalService.GetWithdrawalsByStatusAsync(WithdrawalStatus.Pending,ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(
        Guid id,
        [FromBody] ApproveWithdrawalRequest request,
        CancellationToken ct)
    {
        var adminId = GetUserId();
        var result = await withdrawalService.ApproveAsync(id, adminId, request.AdminNotes, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(
        Guid id,
        [FromBody] RejectWithdrawalRequest request,
        CancellationToken ct)
    {
        var adminId = GetUserId();
        var result = await withdrawalService.RejectAsync(id, adminId, request.Reason, ct);
        return StatusCode(result.StatusCode, result);
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? throw new UnauthorizedAccessException("User ID not found in claims."));
}