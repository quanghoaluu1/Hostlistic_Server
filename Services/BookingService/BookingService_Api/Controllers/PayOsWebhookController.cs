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

    [HttpPost("test-push/{orderId}")]
    public async Task<IActionResult> TestPush(
        Guid orderId,
        [FromServices] IPaymentNotifier notifier)
    {
        await notifier.NotifyPaymentConfirmedAsync(orderId, new PaymentConfirmedPayload
        {
            OrderId = orderId,
            OrderCode = 123456789,
            Status = "Confirmed",
            TotalAmount = 500000,
            Tickets =
            [
                new TicketSummaryDto
                {
                    Id = Guid.NewGuid(),
                    TicketCode = "TKT-TEST001",
                    TicketTypeName = "VIP",
                    QrCodeUrl = "https://via.placeholder.com/200",
                    Price = 500000
                }
            ]
        });
        return Ok("Pushed!");
    }
}