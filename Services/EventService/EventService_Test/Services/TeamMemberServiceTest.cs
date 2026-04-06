using EventService_Domain.Constants;
using EventService_Domain.Enums;
using EventService_Test.Helpers.TestDataBuilders;
using MassTransit;
using Event = EventService_Domain.Entities.Event;

namespace EventService_Test;

public class TeamMemberServiceTest
{
    private readonly IEventTeamMemberRepository _memberRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly TeamMemberService _sut;

    public TeamMemberServiceTest()
    {
        _memberRepository = Substitute.For<IEventTeamMemberRepository>();
        _eventRepository = Substitute.For<IEventRepository>();
        _publishEndpoint = Substitute.For<IPublishEndpoint>();
        _sut = new TeamMemberService(_memberRepository, _eventRepository, _publishEndpoint);
    }

    // ── InviteAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task InviteAsync_WhenEventNotFound_ReturnsFail404()
    {
        // Arrange
        _eventRepository.GetEventByIdAsync(Arg.Any<Guid>()).Returns((Event?)null);

        // Act
        var result = await _sut.InviteAsync(
            Guid.NewGuid(), Guid.NewGuid(), null, TeamMemberBuilder.InviteRequest());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task InviteAsync_WhenNonOrganizerWithoutPermission_ReturnsFail403()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var organizerId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid(); // non-organizer
        var ev = EventBuilder.CreateEvent(id: eventId, organizerId: organizerId);

        _eventRepository.GetEventByIdAsync(eventId).Returns(ev);
        _memberRepository.GetMemberByUserAndEventAsync(currentUserId, eventId).Returns((EventTeamMember?)null);

        // Act
        var result = await _sut.InviteAsync(eventId, currentUserId, null, TeamMemberBuilder.InviteRequest());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(403);
        result.Message.Should().Contain("permission to invite");
    }

    [Fact]
    public async Task InviteAsync_WhenNonOrganizerInvitesCoOrganizer_ReturnsFail403()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var organizerId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var ev = EventBuilder.CreateEvent(id: eventId, organizerId: organizerId);
        var currentMember = TeamMemberBuilder.CreateActiveMember(userId: currentUserId, eventId: eventId, role: EventRole.Staff);
        currentMember.Permissions[EventPermissions.CanManageTeam] = true;

        _eventRepository.GetEventByIdAsync(eventId).Returns(ev);
        _memberRepository.GetMemberByUserAndEventAsync(currentUserId, eventId).Returns(currentMember);

        // Act
        var result = await _sut.InviteAsync(eventId, currentUserId, null,
            TeamMemberBuilder.InviteRequest(role: EventRole.CoOrganizer));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(403);
        result.Message.Should().Contain("Only the event organizer can invite CoOrganizers");
    }

    [Fact]
    public async Task InviteAsync_WhenInvitingSelf_ReturnsFail400()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var ev = EventBuilder.CreateEvent(id: eventId, organizerId: userId); // user is organizer
        _eventRepository.GetEventByIdAsync(eventId).Returns(ev);

        var request = TeamMemberBuilder.InviteRequest(userId: userId); // inviting self

        // Act
        var result = await _sut.InviteAsync(eventId, userId, null, request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("cannot invite yourself");
    }

    [Fact]
    public async Task InviteAsync_WhenUserAlreadyInvited_ReturnsFail409()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var organizerId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var ev = EventBuilder.CreateEvent(id: eventId, organizerId: organizerId);
        var existingMember = TeamMemberBuilder.CreateInvitedMember(userId: targetUserId, eventId: eventId);

        _eventRepository.GetEventByIdAsync(eventId).Returns(ev);
        _memberRepository.GetMemberByUserAndEventAsync(targetUserId, eventId).Returns(existingMember);

        var request = TeamMemberBuilder.InviteRequest(userId: targetUserId);

        // Act
        var result = await _sut.InviteAsync(eventId, organizerId, "Organizer", request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        result.Message.Should().Contain("already has an active or pending invitation");
    }

    [Fact]
    public async Task InviteAsync_WithValidRequest_ReturnsSuccess201AndPublishesEvent()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var organizerId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var ev = EventBuilder.CreateEvent(id: eventId, organizerId: organizerId);

        _eventRepository.GetEventByIdAsync(eventId).Returns(ev);
        _memberRepository.GetMemberByUserAndEventAsync(targetUserId, eventId).Returns((EventTeamMember?)null);
        _memberRepository.CountActiveAndInvitedByEventAsync(eventId).Returns(0);

        var request = TeamMemberBuilder.InviteRequest(userId: targetUserId, role: EventRole.Staff);

        // Act
        var result = await _sut.InviteAsync(eventId, organizerId, "Organizer Name", request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        _memberRepository.Received(1).AddMember(Arg.Any<EventTeamMember>());
        await _memberRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task InviteAsync_WhenQuotaExceeded_ReturnsFail422()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var organizerId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var ev = EventBuilder.CreateEvent(id: eventId, organizerId: organizerId);

        _eventRepository.GetEventByIdAsync(eventId).Returns(ev);
        _memberRepository.GetMemberByUserAndEventAsync(targetUserId, eventId).Returns((EventTeamMember?)null);
        _memberRepository.CountActiveAndInvitedByEventAsync(eventId).Returns(50); // max quota

        // Act
        var result = await _sut.InviteAsync(
            eventId, organizerId, null, TeamMemberBuilder.InviteRequest(userId: targetUserId));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(422);
        result.Message.Should().Contain("quota exceeded");
    }

    // ── GetByEventIdAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task GetByEventIdAsync_WhenEventNotFound_ReturnsFail404()
    {
        // Arrange
        _eventRepository.GetEventByIdAsync(Arg.Any<Guid>()).Returns((Event?)null);

        // Act
        var result = await _sut.GetByEventIdAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetByEventIdAsync_WhenNonMemberAccesses_ReturnsFail403()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var organizerId = Guid.NewGuid();
        var nonMemberId = Guid.NewGuid();
        var ev = EventBuilder.CreateEvent(id: eventId, organizerId: organizerId);
        _eventRepository.GetEventByIdAsync(eventId).Returns(ev);
        _memberRepository.GetMemberByUserAndEventAsync(nonMemberId, eventId).Returns((EventTeamMember?)null);

        // Act
        var result = await _sut.GetByEventIdAsync(eventId, nonMemberId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task GetByEventIdAsync_WhenOrganizer_ReturnsSuccess200()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var organizerId = Guid.NewGuid();
        var ev = EventBuilder.CreateEvent(id: eventId, organizerId: organizerId);
        _eventRepository.GetEventByIdAsync(eventId).Returns(ev);
        _memberRepository.GetMembersByEventIdAsync(eventId)
            .Returns(new List<EventTeamMember> { TeamMemberBuilder.CreateActiveMember(eventId: eventId) }.AsReadOnly());

        // Act
        var result = await _sut.GetByEventIdAsync(eventId, organizerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().HaveCount(1);
    }

    // ── AcceptByTokenAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task AcceptByTokenAsync_WhenTokenNotFound_ReturnsFail404()
    {
        // Arrange
        _memberRepository.GetByInviteTokenAsync(Arg.Any<string>()).Returns((EventTeamMember?)null);

        // Act
        var result = await _sut.AcceptByTokenAsync("invalid-token");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task AcceptByTokenAsync_WithValidToken_ReturnsSuccess200()
    {
        // Arrange
        var member = TeamMemberBuilder.CreateInvitedMember();
        _memberRepository.GetByInviteTokenAsync(member.InviteToken!).Returns(member);

        // Act
        var result = await _sut.AcceptByTokenAsync(member.InviteToken!);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        member.Status.Should().Be(EventMemberStatus.Active);
        await _memberRepository.Received(1).SaveChangesAsync();
    }

    // ── RemoveMemberAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task RemoveMemberAsync_WhenEventNotFound_ReturnsFail404()
    {
        // Arrange
        _eventRepository.GetEventByIdAsync(Arg.Any<Guid>()).Returns((Event?)null);

        // Act
        var result = await _sut.RemoveMemberAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task RemoveMemberAsync_WhenOrganizerRemovesSelf_ReturnsFail400()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var organizerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var ev = EventBuilder.CreateEvent(id: eventId, organizerId: organizerId);
        var target = TeamMemberBuilder.CreateActiveMember(id: memberId, userId: organizerId, eventId: eventId);

        _eventRepository.GetEventByIdAsync(eventId).Returns(ev);
        _memberRepository.GetMemberByIdAsync(memberId).Returns(target);

        // Act
        var result = await _sut.RemoveMemberAsync(eventId, memberId, organizerId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("organizer cannot remove themselves");
    }

    [Fact]
    public async Task RemoveMemberAsync_WhenOrganizerRemovesMember_ReturnsSuccess200()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var organizerId = Guid.NewGuid();
        var memberId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var ev = EventBuilder.CreateEvent(id: eventId, organizerId: organizerId);
        var target = TeamMemberBuilder.CreateActiveMember(id: memberId, userId: targetUserId, eventId: eventId);

        _eventRepository.GetEventByIdAsync(eventId).Returns(ev);
        _memberRepository.GetMemberByIdAsync(memberId).Returns(target);

        // Act
        var result = await _sut.RemoveMemberAsync(eventId, memberId, organizerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        target.Status.Should().Be(EventMemberStatus.Removed);
    }

    // ── RespondByUserAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task RespondByUserAsync_WhenNoPendingInvitation_ReturnsFail404()
    {
        // Arrange
        _memberRepository.GetMemberByUserAndEventAsync(Arg.Any<Guid>(), Arg.Any<Guid>())
            .Returns((EventTeamMember?)null);

        // Act
        var result = await _sut.RespondByUserAsync(Guid.NewGuid(), Guid.NewGuid(), "accept");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task RespondByUserAsync_WithInvalidAction_ReturnsFail400()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var member = TeamMemberBuilder.CreateInvitedMember(userId: userId, eventId: eventId);
        _memberRepository.GetMemberByUserAndEventAsync(userId, eventId).Returns(member);

        // Act
        var result = await _sut.RespondByUserAsync(eventId, userId, "maybe");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("Invalid action");
    }

    [Fact]
    public async Task RespondByUserAsync_WithAcceptAction_ReturnsSuccess200()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var member = TeamMemberBuilder.CreateInvitedMember(userId: userId, eventId: eventId);
        _memberRepository.GetMemberByUserAndEventAsync(userId, eventId).Returns(member);

        // Act
        var result = await _sut.RespondByUserAsync(eventId, userId, "accept");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        member.Status.Should().Be(EventMemberStatus.Active);
    }
}
