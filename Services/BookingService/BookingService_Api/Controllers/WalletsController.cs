using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingService_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WalletsController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletsController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    [HttpGet("{walletId:guid}")]
    public async Task<IActionResult> GetWalletById(Guid walletId)
    {
        var result = await _walletService.GetWalletByIdAsync(walletId);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetWalletByUserId(Guid userId)
    {
        var result = await _walletService.GetWalletByUserIdAsync(userId);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateWallet([FromBody] CreateWalletRequest request)
    {
        var result = await _walletService.CreateWalletAsync(request);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("{walletId:guid}/balance")]
    public async Task<IActionResult> UpdateWalletBalance(Guid walletId, [FromBody] UpdateWalletBalanceRequest request)
    {
        var result = await _walletService.UpdateWalletBalanceAsync(walletId, request);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{walletId:guid}")]
    public async Task<IActionResult> DeleteWallet(Guid walletId)
    {
        var result = await _walletService.DeleteWalletAsync(walletId);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }
}