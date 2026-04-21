using FluentAssertions;
using NSubstitute;
using StreamingService_Application.Interfaces;
using StreamingService_Infrastructure.Services;
using System.Net;

namespace StreamingService_Test.Services.Clients;

public class ClientServiceTests
{
    private readonly MockHttpMessageHandler _msgHandler = new();

    [Fact]
    public async Task EventServiceClient_VerifyStreamAccessAsync_WhenSuccessful_ReturnsData()
    {
        // Arrange
        var json = "{\"isSuccess\":true, \"data\":{\"isAllowed\":true, \"role\":\"Attendee\"}}";
        _msgHandler.Response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) };
        var httpClient = new HttpClient(_msgHandler) { BaseAddress = new Uri("http://event-service") };
        var sut = new EventServiceClient(httpClient);

        // Act
        var result = await sut.VerifyStreamAccessAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.Should().NotBeNull();
        result.IsAllowed.Should().BeTrue();
    }

    [Fact]
    public async Task BookingServiceClient_ValidateGuestLiveTicketAsync_WhenNotFound_ReturnsNull()
    {
        // Arrange
        _msgHandler.Response = new HttpResponseMessage(HttpStatusCode.NotFound);
        var httpClient = new HttpClient(_msgHandler) { BaseAddress = new Uri("http://booking-service") };
        var sut = new BookingServiceClient(httpClient);

        // Act
        var result = await sut.ValidateGuestLiveTicketAsync(Guid.NewGuid(), "T1");

        // Assert
        result.Should().BeNull();
    }

    private class MockHttpMessageHandler : HttpMessageHandler
    {
        public HttpResponseMessage Response { get; set; } = new(HttpStatusCode.OK);
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return await Task.FromResult(Response);
        }
    }
}
