using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Common;
using Common.DTOs;
using Google.Apis.Auth;
using IdentityService_Application.DTOs;
using IdentityService_Application.Interfaces;
using IdentityService_Domain.Entities;
using IdentityService_Domain.Enum;
using IdentityService_Domain.Interfaces;
using Mapster;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NotificationService_Application.Interfaces;

namespace IdentityService_Application.Services;

public class AuthService(IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository, IConfiguration configuration, IOtpService otpService, IHttpClientFactory httpClientFactory)
    : IAuthService
{
    public async Task<ApiResponse<bool>> RegisterAsync(RegisterRequest request)
    {
        if (await userRepository.IsExistByEmailAsync(request.Email)) return ApiResponse<bool>.Fail(400,"Email already exists");
        var newUser = request.Adapt<User>();
        newUser.HashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
        newUser.Role = Role.Member;
        await userRepository.AddUserAsync(newUser);
        await userRepository.SaveChangesAsync();
        return ApiResponse<bool>.Success(201,"User created successfully", true);
    }
    
    public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var user = await userRepository.GetUserByEmailAsync(request.Email);
        if (user is null) 
            return ApiResponse<AuthResponse>.Fail(401,"Invalid credentials");
        
        var isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.HashedPassword);
        if (!isValidPassword)
            return ApiResponse<AuthResponse>.Fail(401,"Invalid credentials");

        if (!user.IsActive)
            return ApiResponse<AuthResponse>.Fail(401, "Account is deactivated");

        var accessToken = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken(user.Id);
        
        await refreshTokenRepository.AddTokenAsync(refreshToken.Entity);
        await refreshTokenRepository.SaveChangesAsync();
        
        var response = new AuthResponse()
        {
            AccessToken = accessToken,
            AccessTokenExpiration = DateTime.UtcNow.AddMinutes(30),
            User = user.Adapt<UserDto>()
        };
        return ApiResponse<AuthResponse>.Success(200, refreshToken.tokenString, response);
    }

    public async Task<ApiResponse<AuthResponse>> RequestPasswordResetAsync(string email)
    {
        var user = await userRepository.GetUserByEmailAsync(email);
        if (user is null)
        {
            return ApiResponse<AuthResponse>.Fail(404, "User not found");
        }

        var otp = await otpService.GenerateOtpAsync(email);
        var client = httpClientFactory.CreateClient();
        var emailRequest = new {Email = email, Otp = otp};
        await client.PostAsJsonAsync("http://localhost:5097/api/Email/send-email-otp", emailRequest);
        return ApiResponse<AuthResponse>.Success(200, "Otp sent successfully", null);
    }

    public async Task<ApiResponse<AuthResponse>> ResetPasswordAsync(string email, string otp, string newPassword)
    {
        var isValidOtp = await otpService.VerifyOtpAsync(email, otp);
        if (!isValidOtp) return ApiResponse<AuthResponse>.Fail(400, "Invalid otp");
        var user = await userRepository.GetUserByEmailAsync(email);
        user.HashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await userRepository.SaveChangesAsync();
        return ApiResponse<AuthResponse>.Success(200, "Password reset successfully", null);
    }

    public async Task<ApiResponse<AuthResponse>> GoogleLoginAsync(GoogleLoginRequest request)
    {
        GoogleJsonWebSignature.Payload payload;
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience =
                [
                    configuration["Google:ClientId"]
                    ?? throw new InvalidOperationException("Google ClientId not configured")
                ]
            };

            payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
        }
        catch (InvalidJwtException)
        {
            return ApiResponse<AuthResponse>.Fail(401, "Invalid Google token");
        }

        var googleId = payload.Subject;
        var email = payload.Email;
        var fullName = payload.Name ?? email;
        var avatarUrl = payload.Picture ?? string.Empty;
        if (!payload.EmailVerified)
        {
            return ApiResponse<AuthResponse>.Fail(400, "Google email is not verified");
        }
        var user = await userRepository.GetUserByEmailAsync(email);
        if (user is not null)
        {
            user.GoogleId = googleId;
            user.LoginProvider = LoginProvider.Google;
            if (string.IsNullOrEmpty(user.AvatarUrl)) user.AvatarUrl = avatarUrl;
            user.UpdatedAt = DateTime.UtcNow;
            await userRepository.UpdateUserAsync(user);
            await userRepository.SaveChangesAsync();
        }
        else
        {
            user = new User()
            {
                Id = Guid.NewGuid(),
                Email = email,
                FullName = fullName,
                AvatarUrl = avatarUrl,
                GoogleId = googleId,
                LoginProvider = LoginProvider.Google,
                HashedPassword = string.Empty,
                Role = Role.Member,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await userRepository.AddUserAsync(user);
            await userRepository.SaveChangesAsync();
        }
        if (!user.IsActive) return ApiResponse<AuthResponse>.Fail(400, "User is deactivated");
        var accessToken = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken(user.Id);

        await refreshTokenRepository.AddTokenAsync(refreshToken.Entity);
        await refreshTokenRepository.SaveChangesAsync();

        var response = new AuthResponse
        {
            AccessToken = accessToken,
            AccessTokenExpiration = DateTime.UtcNow.AddMinutes(30),
            User = user.Adapt<UserDto>()
        };
        return ApiResponse<AuthResponse>.Success(200, refreshToken.tokenString, response);
    }
    
    
    public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync(string oldToken)
    {

            var existingToken = await refreshTokenRepository.GetTokenAsync(oldToken);
            if (existingToken is null || existingToken.IsRevoked || existingToken.ExpiryDate <= DateTime.UtcNow)
            {
                return ApiResponse<AuthResponse>.Fail(401, "Invalid or expired refresh token");
            }

            var user = existingToken.User;
            if (user is null)
                return ApiResponse<AuthResponse>.Fail(401, "Invalid refresh token");

            if (!user.IsActive)
                return ApiResponse<AuthResponse>.Fail(401, "Account is deactivated");

            var newAccessToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken(user.Id);

            await refreshTokenRepository.RevokeRefreshTokenAsync(existingToken);
            await refreshTokenRepository.AddTokenAsync(newRefreshToken.Entity);
            await refreshTokenRepository.SaveChangesAsync();

            var response = new AuthResponse()
            {
                AccessToken = newAccessToken,
                AccessTokenExpiration = DateTime.UtcNow.AddMinutes(30),
                User = user.Adapt<UserDto>()
            };
            return ApiResponse<AuthResponse>.Success(200, newRefreshToken.tokenString, response);
    }

    public async Task<ApiResponse<bool>> LogoutAsync(string refreshToken)
    {
        var existingToken = await refreshTokenRepository.GetTokenAsync(refreshToken);
        if (existingToken is null || existingToken.IsRevoked)
            return ApiResponse<bool>.Fail(400, "Invalid refresh token");

        await refreshTokenRepository.RevokeRefreshTokenAsync(existingToken);
        await refreshTokenRepository.SaveChangesAsync();
        return ApiResponse<bool>.Success(200, "Logged out successfully", true);
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.FullName),
            new Claim("Role", user.Role.ToString())
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt Key not found")));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            configuration["Jwt:Issuer"],
            configuration["Jwt:Audience"],
            claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private (RefreshToken Entity, string tokenString) GenerateRefreshToken(Guid userId)
    {
        var tokenString = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        var entity = new RefreshToken()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = tokenString,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };
        return (entity, tokenString);
    }
}