using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingService_Api.Controllers;

[ApiController]
[Route("api/admin/transactions")]
[Authorize(Roles = "Admin")]
public sealed class AdminTransactionsController(IAdminTransactionService adminTransactionService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] AdminTransactionQueryRequest request,
        CancellationToken ct)
    {
        var result = await adminTransactionService.GetTransactionsAsync(request, ct);
        return StatusCode(result.StatusCode, result);
    }
}
