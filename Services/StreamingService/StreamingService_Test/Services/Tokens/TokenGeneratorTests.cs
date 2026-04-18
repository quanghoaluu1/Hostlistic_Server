using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using StreamingService_Infrastructure.Services;
using StreamingService_Infrastructure.Settings;
using StreamingService_Domain.Enums;
using System.IdentityModel.Tokens.Jwt;

namespace StreamingService_Test.Services.TokenTests;

public class TokenGeneratorTests
{
    private readonly IOptions<LiveKitSettings> _options = Substitute.For<IOptions<LiveKitSettings>>();
    private readonly TokenGenerator _sut;

    public TokenGeneratorTests()
    {
        _options.Value.Returns(new LiveKitSettings
        {
            ApiKey = "test-key",
            ApiSecret = "test-secret-long-enough-for-hmac-sha256-validation",
            ServerUrl = "wss://test.livekit.io"
        });
        _sut = new TokenGenerator(_options);
    }

    [Fact]
    public void GenerateLiveKitToken_ForAttendee_IncludesBasicGrants()
    {
        // Act
        var token = _sut.GenerateLiveKitToken("room1", "user1", ParticipantRole.Attendee);

        // Assert
        token.Should().NotBeNullOrEmpty();
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Issuer.Should().Be("test-key");
        jwt.Subject.Should().Be("user1");
        jwt.Claims.Should().Contain(c => c.Type == "video");
        
        var videoGrant = jwt.Claims.First(c => c.Type == "video").Value;
        videoGrant.Should().Contain("\"room\":\"room1\"");
        videoGrant.Should().Contain("\"canPublish\":false");
        videoGrant.Should().Contain("\"canSubscribe\":true");
    }

    [Fact]
    public void GenerateLiveKitToken_ForHost_IncludesAdminGrants()
    {
        // Act
        var token = _sut.GenerateLiveKitToken("room1", "host1", ParticipantRole.Organizer);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        var videoGrant = jwt.Claims.First(c => c.Type == "video").Value;

        videoGrant.Should().Contain("\"roomAdmin\":true");
        videoGrant.Should().Contain("\"canPublish\":true");
    }
}
