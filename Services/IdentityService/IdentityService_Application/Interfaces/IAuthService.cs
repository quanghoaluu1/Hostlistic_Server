using Common;
using IdentityService_Application.DTOs;

namespace IdentityService_Application.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request);
    Task<ApiResponse<bool>> RegisterAsync(RegisterRequest request);
    Task<ApiResponse<AuthResponse>> RefreshTokenAsync(string oldToken);
    Task<ApiResponse<AuthResponse>> RequestPasswordResetAsync(string email);
    Task<ApiResponse<AuthResponse>> ResetPasswordAsync(string email, string otp, string newPassword);
    Task<ApiResponse<AuthResponse>> GoogleLoginAsync(GoogleLoginRequest request);
    Task<ApiResponse<bool>> LogoutAsync(string refreshToken);
}