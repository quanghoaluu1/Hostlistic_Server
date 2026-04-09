using System.Security.Claims;
using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingService_Api.Controllers;

[ApiController]
[Route("api/withdrawals")]
[Authorize]
public class WithdrawalController(IWithdrawalRequestService withdrawalService) : ControllerBase
{
    [HttpPost("request")]
    public async Task<IActionResult> CreateRequest(
        [FromBody] CreateWithdrawalRequest request,
        CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await withdrawalService.CreateRequestAsync(userId, request, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyWithdrawals(CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await withdrawalService.GetMyWithdrawalsAsync(userId, ct);
        return StatusCode(result.StatusCode, result);
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? throw new UnauthorizedAccessException("User ID not found in claims."));
}
