using Common;
using IdentityService_Application.DTOs;
using IdentityService_Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody]RegisterRequest request)
    {
        var result = await authService.RegisterAsync(request);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await authService.LoginAsync(request);
        if (!result.IsSuccess) return BadRequest(result);
        var refreshToken = result.Message;
        SetRefreshTokenCookie(refreshToken);
        result.Message = "Login successfully";
        return Ok(result);
    }
    
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
            return BadRequest(ApiResponse<string>.Fail(400, "Refresh token is missing"));

        var result = await authService.RefreshTokenAsync(refreshToken);
        
        if (!result.IsSuccess) return BadRequest(result);

        SetRefreshTokenCookie(result.Message);
        result.Message = "Token refreshed successfully";

        return Ok(result);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> SendResetPasswordEmail([FromBody] ForgotPasswordRequest request)
    {
        var result = await authService.RequestPasswordResetAsync(request.Email);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequest request)
    {
        var result = await authService.ResetPasswordAsync(request.Email, request.Otp, request.NewPassword);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }
    
    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        var result = await authService.GoogleLoginAsync(request);
        if (!result.IsSuccess) return BadRequest(result);
        var refreshToken = result.Message;
        SetRefreshTokenCookie(refreshToken);
        result.Message = "Login Google successfully";
        return Ok(result);
    }
    
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
            return BadRequest(ApiResponse<bool>.Fail(400, "Refresh token is missing"));

        var result = await authService.LogoutAsync(refreshToken);
        if (!result.IsSuccess) return BadRequest(result);

        Response.Cookies.Delete("refreshToken");
        return Ok(result);
    }

    private void SetRefreshTokenCookie(string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true, 
            Expires = DateTime.UtcNow.AddDays(7),
            SameSite = SameSiteMode.Strict,
            Secure = true 
        };
        Response.Cookies.Append("refreshToken", token, cookieOptions);
    }
}