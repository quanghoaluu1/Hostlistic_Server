using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StreamingService_Application.Interfaces;
using StreamingService_Domain.Enums;
using StreamingService_Infrastructure.Settings;

namespace StreamingService_Infrastructure.Services;

public class TokenGenerator : ITokenGenerator
{
    private readonly LiveKitSettings _settings;

    public TokenGenerator(IOptions<LiveKitSettings> options)
    {
        _settings = options.Value;
    }

    public string GenerateLiveKitToken(string roomName, string identity, ParticipantRole role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.ApiSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var canPublish = role == ParticipantRole.Organizer || role == ParticipantRole.CoOrganizer;
        var isAdmin = role == ParticipantRole.Organizer || role == ParticipantRole.CoOrganizer || role == ParticipantRole.Staff;

        var grants = new
        {
            roomJoin = true,
            room = roomName,
            canPublish = canPublish,
            canSubscribe = true,
            canPublishData = true,
            roomAdmin = isAdmin
        };

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, identity),
            new Claim("name", identity),
            new Claim("video", JsonSerializer.Serialize(grants), JsonClaimValueTypes.Json)
        };

        var token = new JwtSecurityToken(
            issuer: _settings.ApiKey,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials);

        // Required headers for LiveKit tokens
        token.Header.Remove("typ");
        token.Header.Add("kid", _settings.ApiKey);

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }
}
