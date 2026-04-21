using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using StreamingService_Infrastructure.Services;
using StreamingService_Infrastructure.Settings;
using System.Net;
using System.Net.Http;
using System.Threading;

namespace StreamingService_Test.Services.LiveKitTests;

public class LiveKitServiceTests
{
    private readonly IOptions<LiveKitSettings> _options = Substitute.For<IOptions<LiveKitSettings>>();
    private readonly MockHttpMessageHandler _msgHandler = new();
    private readonly LiveKitService _sut;

    public LiveKitServiceTests()
    {
        _options.Value.Returns(new LiveKitSettings
        {
            ApiKey = "key",
            ApiSecret = "secret-length-must-be-at-least-thirty-two-chars-long",
            ServerUrl = "https://livekit.test"
        });
        var httpClient = new HttpClient(_msgHandler) { BaseAddress = new Uri("https://livekit.test") };
        _sut = new LiveKitService(httpClient, _options);
    }

    [Fact]
    public async Task CreateRoomAsync_WhenSuccessful_ReturnsSuccessResult()
    {
        // Arrange
        _msgHandler.Response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") };

        // Act
        var result = await _sut.CreateRoomAsync("room1", 100);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _msgHandler.LastRequest.RequestUri.ToString().Should().Contain("CreateRoom");
    }

    [Fact]
    public async Task EndRoomAsync_WhenApiFails_ReturnsFailureResult()
    {
        // Arrange
        _msgHandler.Response = new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("Error") };

        // Act
        var result = await _sut.EndRoomAsync("room1");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Bad Request");
    }

    private class MockHttpMessageHandler : HttpMessageHandler
    {
        public HttpResponseMessage Response { get; set; } = new(HttpStatusCode.OK);
        public HttpRequestMessage LastRequest { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return await Task.FromResult(Response);
        }
    }
}
