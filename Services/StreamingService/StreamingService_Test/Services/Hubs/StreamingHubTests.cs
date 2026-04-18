using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using StreamingService_Api.Hubs;
using StreamingService_Application.Interfaces;
using System.Security.Claims;

namespace StreamingService_Test.Services.Hubs;

public class StreamingHubTests
{
    private readonly IEventServiceClient _eventServiceClient = Substitute.For<IEventServiceClient>();
    private readonly StreamingHub _sut;
    private readonly IHubCallerClients _clients = Substitute.For<IHubCallerClients>();
    private readonly IGroupManager _groups = Substitute.For<IGroupManager>();
    private readonly HubCallerContext _context = Substitute.For<HubCallerContext>();

    public StreamingHubTests()
    {
        _sut = new StreamingHub(_eventServiceClient)
        {
            Clients = _clients,
            Groups = _groups,
            Context = _context
        };
    }

    [Fact]
    public async Task JoinEventGroup_AddsConnectionToGroup()
    {
        // Arrange
        var eventId = Guid.NewGuid().ToString();
        _context.ConnectionId.Returns("conn1");

        // Act
        await _sut.JoinEventGroup(eventId);

        // Assert
        await _groups.Received(1).AddToGroupAsync("conn1", Arg.Any<string>());
    }

    [Fact]
    public async Task SendEventChatMessage_WhenUnauthenticated_ThrowsHubException()
    {
        // Arrange
        _context.User.Returns((ClaimsPrincipal)null);

        // Act
        var act = () => _sut.SendEventChatMessage("ev1", "sess1", "user", "hi");

        // Assert
        await act.Should().ThrowAsync<HubException>().WithMessage("You must be authenticated*");
    }

    [Fact]
    public async Task SendEventChatMessage_WhenChatBlocked_CallsCallerNotify()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        SetupUser(userId);
        
        _eventServiceClient.GetEventChatAccessAsync(eventId, sessionId, userId)
            .Returns(new EventChatAccessResponseDto { CanSendChat = false });

        var caller = Substitute.For<ISingleClientProxy>();
        _clients.Caller.Returns(caller);

        // Act
        await _sut.SendEventChatMessage(eventId.ToString(), sessionId.ToString(), "John", "hi");

        // Assert
        await caller.Received(1).SendCoreAsync("EventChatBlocked", Arg.Any<object[]>());
    }

    private void SetupUser(Guid userId)
    {
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) });
        _context.User.Returns(new ClaimsPrincipal(identity));
    }
}
