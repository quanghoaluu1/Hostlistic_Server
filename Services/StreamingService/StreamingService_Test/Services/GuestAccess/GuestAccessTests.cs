using FluentAssertions;
using StreamingService_Application.Interfaces;
using StreamingService_Infrastructure.Services;

namespace StreamingService_Test.Services.GuestAccess;

public class GuestStreamAccessTests
{
    private readonly GuestStreamAccessService _sut = new();

    [Fact]
    public void RegisterFailedAttempt_IncrementsCountAndBlocksAfterLimit()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var clientKey = "127.0.0.1";

        // Act
        for (int i = 0; i < 5; i++)
        {
            _sut.RegisterFailedAttempt(eventId, clientKey);
        }
        var status = _sut.GetAttemptStatus(eventId, clientKey);

        // Assert
        status.FailedAttempts.Should().Be(5);
        status.IsBlocked.Should().BeFalse();

        _sut.RegisterFailedAttempt(eventId, clientKey);
        var blockedStatus = _sut.GetAttemptStatus(eventId, clientKey);
        blockedStatus.IsBlocked.Should().BeTrue();
    }

    [Fact]
    public void CreateOrReplaceSession_CreatesNewSession()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var ticket = new GuestLiveTicketValidationDto { TicketId = Guid.NewGuid(), TicketCode = "T1", HolderName = "John" };

        // Act
        var session = _sut.CreateOrReplaceSession(eventId, roomId, ticket, "Custom Name");

        // Assert
        session.Should().NotBeNull();
        session.HolderName.Should().Be("Custom Name");
        _sut.TryGetActiveSession(ticket.TicketId, out var active).Should().BeTrue();
        active.SessionId.Should().Be(session.SessionId);
    }
}
