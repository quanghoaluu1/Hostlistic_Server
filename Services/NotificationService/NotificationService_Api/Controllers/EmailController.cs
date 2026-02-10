using Microsoft.AspNetCore.Mvc;
using NotificationService_Application.Dtos;
using NotificationService_Application.Interfaces;

namespace NotificationService_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmailController(IEmailService emailService) : ControllerBase
{
    [HttpPost("send-email-otp")]
    public async Task<IActionResult> SendEmail([FromBody]EmailOtpRequest request)
    {
        try
        {
            await emailService.SendOtpEmailAsync(request);
            return Ok("Email sent successfully");
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}