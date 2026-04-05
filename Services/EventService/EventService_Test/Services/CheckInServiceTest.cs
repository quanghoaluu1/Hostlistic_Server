using EventService_Test.Helpers.TestDataBuilders;
using Mapster;

namespace EventService_Test;

public class CheckInServiceTest
{
    private readonly ICheckInRepository _checkInRepository;
    private readonly CheckInService _sut;

    public CheckInServiceTest()
    {
        _checkInRepository = Substitute.For<ICheckInRepository>();
        _sut = new CheckInService(_checkInRepository);

        TypeAdapterConfig<CheckIn, CheckInDto>.NewConfig();
        TypeAdapterConfig<CreateCheckInRequest, CheckIn>.NewConfig();
    }

    // ── GetCheckInByIdAsync ────────────────────────────────────────────────

    [Fact]
    public async Task GetCheckInByIdAsync_WhenNotFound_ReturnsFail404()
    {
        // Arrange
        _checkInRepository.GetCheckInByIdAsync(Arg.Any<Guid>()).Returns((CheckIn?)null);

        // Act
        var result = await _sut.GetCheckInByIdAsync(Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetCheckInByIdAsync_WhenExists_ReturnsSuccess200()
    {
        // Arrange
        var id = Guid.NewGuid();
        var checkIn = CheckInBuilder.CreateEntity(id: id, location: "Main Entrance");
        _checkInRepository.GetCheckInByIdAsync(id).Returns(checkIn);

        // Act
        var result = await _sut.GetCheckInByIdAsync(id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
    }

    // ── GetCheckInByTicketIdAsync ──────────────────────────────────────────

    [Fact]
    public async Task GetCheckInByTicketIdAsync_WhenNoCheckIn_ReturnsSuccessWithNullData()
    {
        // Arrange
        _checkInRepository.GetCheckInByTicketIdAsync(Arg.Any<Guid>()).Returns((CheckIn?)null);

        // Act
        var result = await _sut.GetCheckInByTicketIdAsync(Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetCheckInByTicketIdAsync_WhenCheckInExists_ReturnsSuccessWithData()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var checkIn = CheckInBuilder.CreateEntity(ticketId: ticketId);
        _checkInRepository.GetCheckInByTicketIdAsync(ticketId).Returns(checkIn);

        // Act
        var result = await _sut.GetCheckInByTicketIdAsync(ticketId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    // ── CreateCheckInAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task CreateCheckInAsync_WhenTicketAlreadyCheckedIn_ReturnsFail400()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var existing = CheckInBuilder.CreateEntity(ticketId: ticketId);
        var request = CheckInBuilder.CreateRequest(ticketId: ticketId);
        _checkInRepository.GetCheckInByTicketIdAsync(ticketId).Returns(existing);

        // Act
        var result = await _sut.CreateCheckInAsync(Guid.NewGuid(), request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("already been checked in");
    }

    [Fact]
    public async Task CreateCheckInAsync_WithNewTicket_ReturnsSuccess201()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var request = CheckInBuilder.CreateRequest(ticketId: ticketId);
        _checkInRepository.GetCheckInByTicketIdAsync(ticketId).Returns((CheckIn?)null);

        // Act
        var result = await _sut.CreateCheckInAsync(Guid.NewGuid(), request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Message.Should().Contain("Check-in created successfully");
        await _checkInRepository.Received(1).AddCheckInAsync(Arg.Any<CheckIn>());
        await _checkInRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task CreateCheckInAsync_SetsCheckedByAndTimestamp()
    {
        // Arrange
        var checkedByUserId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var request = CheckInBuilder.CreateRequest(ticketId: ticketId);
        _checkInRepository.GetCheckInByTicketIdAsync(ticketId).Returns((CheckIn?)null);

        CheckIn? captured = null;
        _checkInRepository.AddCheckInAsync(Arg.Do<CheckIn>(c => captured = c));

        // Act
        await _sut.CreateCheckInAsync(checkedByUserId, request);

        // Assert
        captured.Should().NotBeNull();
        captured!.CheckedBy.Should().Be(checkedByUserId);
        captured.CheckedInAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    // ── UpdateCheckInAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task UpdateCheckInAsync_WhenNotFound_ReturnsFail404()
    {
        // Arrange
        _checkInRepository.GetCheckInByIdAsync(Arg.Any<Guid>()).Returns((CheckIn?)null);

        // Act
        var result = await _sut.UpdateCheckInAsync(Guid.NewGuid(), CheckInBuilder.UpdateRequest());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task UpdateCheckInAsync_WithValidRequest_ReturnsSuccess200()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existing = CheckInBuilder.CreateEntity(id: id, location: "Front Gate");
        _checkInRepository.GetCheckInByIdAsync(id).Returns(existing);

        // Act
        var result = await _sut.UpdateCheckInAsync(id, CheckInBuilder.UpdateRequest(location: "Back Gate"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        existing.CheckInLocation.Should().Be("Back Gate");
        await _checkInRepository.Received(1).UpdateCheckInAsync(Arg.Any<CheckIn>());
    }

    // ── DeleteCheckInAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task DeleteCheckInAsync_WhenNotFound_ReturnsFail404()
    {
        // Arrange
        _checkInRepository.CheckInExistsAsync(Arg.Any<Guid>()).Returns(false);

        // Act
        var result = await _sut.DeleteCheckInAsync(Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task DeleteCheckInAsync_WhenExists_ReturnsSuccess200()
    {
        // Arrange
        var id = Guid.NewGuid();
        _checkInRepository.CheckInExistsAsync(id).Returns(true);
        _checkInRepository.DeleteCheckInAsync(id).Returns(true);

        // Act
        var result = await _sut.DeleteCheckInAsync(id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
    }
}
