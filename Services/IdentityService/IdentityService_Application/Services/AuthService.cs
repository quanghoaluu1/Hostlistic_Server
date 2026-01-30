using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Common;
using Common.DTOs;
using IdentityService_Application.DTOs;
using IdentityService_Application.Interfaces;
using IdentityService_Domain.Entities;
using IdentityService_Domain.Enum;
using IdentityService_Domain.Interfaces;
using Mapster;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace IdentityService_Application.Services;

public class AuthService(IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository, IConfiguration configuration)
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
    
    public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync(string oldToken)
    {

            var existingToken = await refreshTokenRepository.GetTokenAsync(oldToken);
            if (existingToken is null || existingToken.IsRevoked || existingToken.ExpiryDate <= DateTime.UtcNow)
            {
                return ApiResponse<AuthResponse>.Fail(401, "Invalid or expired refresh token");
            }

            var user = existingToken.User;
            if (user is null)
            {
                return ApiResponse<AuthResponse>.Fail(401, "Invalid refresh token");
            }

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
            configuration["Jwt:Issuer"], 
            claims,
            expires: DateTime.Now.AddMinutes(30),
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