using Common;
using IdentityService_Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using NotificationService_Application.Interfaces;

namespace IdentityService_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OtpController(IOtpService otpService) : ControllerBase
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