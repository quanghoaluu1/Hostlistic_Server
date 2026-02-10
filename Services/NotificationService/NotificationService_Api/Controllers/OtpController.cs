using Common;
using Microsoft.AspNetCore.Mvc;
using NotificationService_Application.Dtos;
using NotificationService_Application.Interfaces;

namespace NotificationService_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OtpController(IOtpService otpService, IEmailService emailService) : ControllerBase
{
    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
    {
        if (string.IsNullOrEmpty(request.Email))
        {
            return BadRequest(ApiResponse<string>.Fail(400, "Email is required"));
        }
        try
        {
            var otp = await otpService.GenerateOtpAsync(request.Email);
            await emailService.SendOtpEmailAsync(request.Email, otp);
            return Ok(ApiResponse<string>.Success(200, "Otp sent successfully", null));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest(ApiResponse<string>.Fail(500, "Failed to send otp"));
        }
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        var isValid = await otpService.VerifyOtpAsync(request.Email, request.Otp);
        if (!isValid) return BadRequest(ApiResponse<string>.Fail(400, "Invalid otp"));
        return Ok(ApiResponse<string>.Success(200, "Otp verified successfully", null));
    }
}