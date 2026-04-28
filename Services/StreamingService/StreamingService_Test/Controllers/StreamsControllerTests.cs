using StreamingService_Test.Helpers.TestDataBuilders;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using StreamingService_Api.Controllers;
using StreamingService_Api.Hubs;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace StreamingService_Test.Controllers;

public class StreamsControllerTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly IStreamingServiceDbContext _dbContext = StreamRoomBuilder.CreateInMemoryDbContext();
    private readonly IHubContext<StreamingHub> _hubContext = Substitute.For<IHubContext<StreamingHub>>();
    private readonly IEventServiceClient _eventServiceClient = Substitute.For<IEventServiceClient>();
    private readonly IBookingServiceClient _bookingServiceClient = Substitute.For<IBookingServiceClient>();
    private readonly IGuestStreamAccessService _guestStreamAccessService = Substitute.For<IGuestStreamAccessService>();
    private readonly ITokenGenerator _tokenGenerator = Substitute.For<ITokenGenerator>();
    
    private readonly StreamsController _sut;

    public StreamsControllerTests()
    {
        _sut = new StreamsController(
            _mediator,
            _dbContext,
            _hubContext,
            _eventServiceClient,
            _bookingServiceClient,
            _guestStreamAccessService,
            _tokenGenerator);
            
        // Mock User identity for Controller property
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        }, "TestAuthentication"));

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task CreateStreamRoom_ReturnsOk()
    {
        // Arrange
        var command = StreamRoomBuilder.CreateCommand();
        _mediator.Send(Arg.Any<CreateStreamRoomCommand>()).Returns(Guid.NewGuid());
        
        var mockClients = Substitute.For<IHubClients>();
        var mockClientProxy = Substitute.For<IClientProxy>();
        _hubContext.Clients.Returns(mockClients);
        mockClients.Group(Arg.Any<string>()).Returns(mockClientProxy);

        // Act
        var result = await _sut.CreateStreamRoom(command);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task CreateGuestAccess_ValidTicket_ReturnsOk()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var request = new GuestStreamAccessRequest { TicketCode = "TC-123", HolderName = "Guest User" };
        var room = StreamRoomBuilder.CreateRoom(eventId: eventId, status: StreamRoomStatus.Live);
        
        _dbContext.StreamRooms.Add(room);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _guestStreamAccessService.GetAttemptStatus(eventId, Arg.Any<string>())
            .Returns(new GuestLiveAttemptStatus { IsBlocked = false });
        
        _bookingServiceClient.ValidateGuestLiveTicketAsync(eventId, Arg.Any<string>())
            .Returns(new GuestLiveTicketValidationDto { TicketId = Guid.NewGuid(), TicketCode = "TC123", TicketTypeId = Guid.NewGuid() });
        _eventServiceClient.GetTicketTypeStreamingAccessAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new TicketTypeStreamingAccessDto());

        _guestStreamAccessService.CreateOrReplaceSession(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<GuestLiveTicketValidationDto>(), Arg.Any<string>())
            .Returns(new GuestLiveSession { SessionId = Guid.NewGuid(), Identity = "guest-123" });

        // Act
        var result = await _sut.CreateGuestAccess(eventId, request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task CreateGuestAccess_WhenClientBlocked_ReturnsTooManyRequests()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var request = new GuestStreamAccessRequest { TicketCode = "T-12" };
        
        _guestStreamAccessService.GetAttemptStatus(eventId, Arg.Any<string>())
            .Returns(new GuestLiveAttemptStatus { IsBlocked = true });

        // Act
        var result = await _sut.CreateGuestAccess(eventId, request);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(429);
        result.As<ObjectResult>().Value.As<object>().ToString().Should().Contain("Too many invalid ticket code attempts");
    }

    [Fact]
    public async Task CreateGuestAccess_WhenTicketAlreadyUsed_ReturnsConflict()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var request = new GuestStreamAccessRequest { TicketCode = "T-USED" };
        var room = StreamRoomBuilder.CreateRoom(eventId: eventId, roomName: "live-room", status: StreamRoomStatus.Live);

        _guestStreamAccessService.GetAttemptStatus(eventId, Arg.Any<string>())
            .Returns(new GuestLiveAttemptStatus { IsBlocked = false });
        
        _dbContext.StreamRooms.Add(room);
        await _dbContext.SaveChangesAsync(CancellationToken.None);
        _bookingServiceClient.ValidateGuestLiveTicketAsync(eventId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new GuestLiveTicketValidationDto { TicketId = Guid.NewGuid(), TicketCode = "T-USED", TicketTypeId = Guid.NewGuid() });
        _eventServiceClient.GetTicketTypeStreamingAccessAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new TicketTypeStreamingAccessDto());
        
        _guestStreamAccessService.TryGetActiveSession(Arg.Any<Guid>(), out Arg.Any<GuestLiveSession?>())
            .Returns(x => { x[1] = new GuestLiveSession { SessionId = Guid.NewGuid() }; return true; });

        // Act
        var result = await _sut.CreateGuestAccess(eventId, request);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task CreateGuestAccess_WhenTrackNotAllowed_ReturnsForbidden()
    {
        var eventId = Guid.NewGuid();
        var allowedTrackId = Guid.NewGuid();
        var request = new GuestStreamAccessRequest { TicketCode = "TRACK-LOCKED" };
        var room = StreamRoomBuilder.CreateRoom(eventId: eventId, status: StreamRoomStatus.Live);
        room.TrackId = Guid.NewGuid();

        _guestStreamAccessService.GetAttemptStatus(eventId, Arg.Any<string>())
            .Returns(new GuestLiveAttemptStatus { IsBlocked = false });

        _dbContext.StreamRooms.Add(room);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _bookingServiceClient.ValidateGuestLiveTicketAsync(eventId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new GuestLiveTicketValidationDto
            {
                TicketId = Guid.NewGuid(),
                TicketCode = "TRACK-LOCKED",
                TicketTypeId = Guid.NewGuid()
            });

        _eventServiceClient.GetTicketTypeStreamingAccessAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new TicketTypeStreamingAccessDto
            {
                AllowedTrackIds = [allowedTrackId]
            });

        var result = await _sut.CreateGuestAccess(eventId, request);

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task UploadRecording_WhenCalled_ReturnsGone()
    {
        // Act
        var result = _sut.UploadRecording(Guid.NewGuid());

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status410Gone);
    }

    [Fact]
    public void UploadRecording_WhenCalled_ReturnsMessageAboutServerSidePipeline()
    {
        var roomId = Guid.NewGuid();

        // Act
        var result = _sut.UploadRecording(roomId);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.Value.As<object>().ToString().Should().Contain("server-side recording pipeline");
    }
}
