using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StreamingService_Application.Interfaces;
using StreamingService_Infrastructure.Settings;

namespace StreamingService_Infrastructure.Services;

public class LiveKitService : ILiveKitService
{
    private readonly HttpClient _httpClient;
    private readonly LiveKitSettings _settings;

    public LiveKitService(HttpClient httpClient, IOptions<LiveKitSettings> options)
    {
        _httpClient = httpClient;
        _settings = options.Value;
    }

    public async Task<LiveKitOperationResult> CreateRoomAsync(string roomName, int maxParticipants, CancellationToken cancellationToken = default)
    {
        var requestData = new 
        {
            name = roomName,
            empty_timeout = 5 * 60, // 5 minutes timeout if empty
            max_participants = (uint)maxParticipants
        };

        return await SendLiveKitApiRequestAsync("RoomService", "CreateRoom", requestData, cancellationToken);
    }

    public async Task<LiveKitOperationResult> EndRoomAsync(string roomName, CancellationToken cancellationToken = default)
    {
        var requestData = new 
        {
            room = roomName
        };

        return await SendLiveKitApiRequestAsync("RoomService", "DeleteRoom", requestData, cancellationToken);
    }

    private async Task<LiveKitOperationResult> SendLiveKitApiRequestAsync(string service, string method, object data, CancellationToken cancellationToken)
    {
        // 1. Generate auth token for LiveKit API
        // LiveKit requires a token with roomCreate claim to manage rooms -> reusing Token Generator by adding special grants could be done, but for simplicity let's generate a dedicated management token.
        var managementToken = GenerateManagementToken();

        // 2. Prepare HTTP request
        var url = $"{GetAdminApiBaseUrl()}/twirp/livekit.{service}/{method}";
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", managementToken);

        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        // 3. Send
        try
        {
            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return new LiveKitOperationResult(true);
            }

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            var message = string.IsNullOrWhiteSpace(responseBody)
                ? $"LiveKit API returned {(int)response.StatusCode} {response.ReasonPhrase}."
                : $"LiveKit API returned {(int)response.StatusCode} {response.ReasonPhrase}: {responseBody}";

            return new LiveKitOperationResult(false, message);
        }
        catch (Exception ex)
        {
            return new LiveKitOperationResult(false, ex.Message);
        }
    }

    private string GenerateManagementToken()
    {
        // For API calls, it needs roomCreate and roomList permissions
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.ApiSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("sub", "streaming-service"),
            new Claim("video", JsonSerializer.Serialize(new { roomCreate = true, roomList = true, roomRecord = true, roomAdmin = true }), System.IdentityModel.Tokens.Jwt.JsonClaimValueTypes.Json)
        };

        var tokenInternal = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: _settings.ApiKey,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: credentials);
            
        tokenInternal.Header.Add("kid", _settings.ApiKey);

        return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(tokenInternal);
    }

    private string GetAdminApiBaseUrl()
    {
        if (!Uri.TryCreate(_settings.ServerUrl, UriKind.Absolute, out var serverUri))
            throw new InvalidOperationException("LiveKit ServerUrl is invalid.");

        var builder = new UriBuilder(serverUri);
        builder.Scheme = builder.Scheme switch
        {
            "wss" => "https",
            "ws" => "http",
            _ => builder.Scheme
        };
        builder.Path = string.Empty;
        builder.Query = string.Empty;

        return builder.Uri.ToString().TrimEnd('/');
    }
}
