using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingService_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionsController(ISubscriptionPurchaseService subscriptionPurchaseService) : ControllerBase
{
    [HttpPost("purchase-with-wallet")]
    public async Task<IActionResult> PurchaseWithWallet([FromBody] PurchaseSubscriptionWithWalletRequest request)
    {
        var result = await subscriptionPurchaseService.PurchaseWithWalletAsync(request);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
}
