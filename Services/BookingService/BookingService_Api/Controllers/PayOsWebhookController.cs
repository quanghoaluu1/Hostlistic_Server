using System.Text.Json;
using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayOS.Models.Webhooks;

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
    public async Task<IActionResult> HandleWebhook()
    {
        Request.EnableBuffering();
        Request.Body.Position = 0;
        var rawBody = await new StreamReader(Request.Body).ReadToEndAsync();
        Request.Body.Position = 0;

        logger.LogInformation("PayOS raw payload: {Payload}", rawBody);

        // Controller chỉ gọi interface — không biết PayOSClient
        var result = await payOsService.HandleWebhookAsync(rawBody);

        if (result is null)
        {
            logger.LogInformation("Webhook ping or non-payment event");
            return Ok();
        }

        if (!result.IsVerified)
        {
            logger.LogWarning("Invalid webhook signature");
            return Unauthorized();
        }

        if (result.IsSuccess)
        {
            await webhookHandler.HandlePaymentSuccessAsync(result.Data!);
        }

        return Ok();
    }

}