using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingService_Api.Controllers;

[ApiController]
[Route("api/payos-webhook")]
[AllowAnonymous]
public class PayOsWebhookController(
    IPayOsService payOsService,
    IPayOsWebhookHandler webhookHandler, 
    ILogger<PayOsWebhookController> logger
    ) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> HandleWebhook([FromBody] PayOsWebhookPayload payload)
    {
        logger.LogInformation("PayOS webhook received, orderCode: {OrderCode}",
            payload.Data?.OrderCode);

        // 1. SDK verify signature
        var verifiedData = await payOsService.VerifyWebhookAsync(payload);
        if (verifiedData is null)
        {
            logger.LogWarning("Invalid webhook signature");
            return Unauthorized();
        }

        // 2. Chỉ xử lý payment thành công
        if (payload.Code != "00")
        {
            logger.LogInformation("Non-success webhook code: {Code}", payload.Code);
            return Ok();
        }

        // 3. Process
        await webhookHandler.HandlePaymentSuccessAsync(payload.Data!);

        return Ok();
    }

}