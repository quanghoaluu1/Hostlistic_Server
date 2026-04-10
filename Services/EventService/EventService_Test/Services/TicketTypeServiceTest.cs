using EventService_Test.Helpers.TestDataBuilders;
using Mapster;

namespace EventService_Test;

public class TicketTypeServiceTest
{
    private readonly ITicketTypeRepository _ticketTypeRepository;
    private readonly TicketTypeService _sut;

    public TicketTypeServiceTest()
    {
        _ticketTypeRepository = Substitute.For<ITicketTypeRepository>();
        _sut = new TicketTypeService(_ticketTypeRepository);

        TypeAdapterConfig<TicketType, TicketTypeDto>.NewConfig();
        TypeAdapterConfig<CreateTicketTypeRequest, TicketType>.NewConfig();
    }

    // ── GetTicketTypeByIdAsync ─────────────────────────────────────────────

    [Fact]
    public async Task GetTicketTypeByIdAsync_WhenNotFound_ReturnsFail404()
    {
        // Arrange
        _ticketTypeRepository.GetTicketTypeByIdAsync(Arg.Any<Guid>()).Returns((TicketType?)null);

        // Act
        var result = await _sut.GetTicketTypeByIdAsync(Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetTicketTypeByIdAsync_WhenExists_ReturnsSuccess200()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ticketType = TicketTypeBuilder.CreateEntity(id: id, name: "Early Bird");
        _ticketTypeRepository.GetTicketTypeByIdAsync(id).Returns(ticketType);

        // Act
        var result = await _sut.GetTicketTypeByIdAsync(id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data!.Name.Should().Be("Early Bird");
    }

    // ── CreateTicketTypeAsync ──────────────────────────────────────────────

    [Fact]
    public async Task CreateTicketTypeAsync_WithEmptyName_ReturnsFail400()
    {
        // Arrange
        var request = TicketTypeBuilder.CreateRequest(name: "   ");

        // Act
        var result = await _sut.CreateTicketTypeAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("Ticket type name is required");
    }

    [Fact]
    public async Task CreateTicketTypeAsync_WithNegativePrice_ReturnsFail400()
    {
        // Arrange
        var request = TicketTypeBuilder.CreateRequest(price: -10);

        // Act
        var result = await _sut.CreateTicketTypeAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("Price must be greater than or equal to 0");
    }

    [Fact]
    public async Task CreateTicketTypeAsync_WithZeroQuantity_ReturnsFail400()
    {
        // Arrange
        var request = TicketTypeBuilder.CreateRequest(quantity: 0);

        // Act
        var result = await _sut.CreateTicketTypeAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("Quantity available must be greater than 0");
    }

    [Fact]
    public async Task CreateTicketTypeAsync_WithMinGreaterThanMax_ReturnsFail400()
    {
        // Arrange
        var request = TicketTypeBuilder.CreateRequest(minPerOrder: 10, maxPerOrder: 5);

        // Act
        var result = await _sut.CreateTicketTypeAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task CreateTicketTypeAsync_WithValidRequest_ReturnsSuccess201()
    {
        // Arrange
        var request = TicketTypeBuilder.CreateRequest();

        // Act
        var result = await _sut.CreateTicketTypeAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Message.Should().Contain("Ticket type created successfully");
        await _ticketTypeRepository.Received(1).AddTicketTypeAsync(Arg.Any<TicketType>());
        await _ticketTypeRepository.Received(1).SaveChangesAsync();
    }

    // ── UpdateTicketTypeAsync ──────────────────────────────────────────────

    [Fact]
    public async Task UpdateTicketTypeAsync_WhenNotFound_ReturnsFail404()
    {
        // Arrange
        _ticketTypeRepository.GetTicketTypeByIdAsync(Arg.Any<Guid>()).Returns((TicketType?)null);

        // Act
        var result = await _sut.UpdateTicketTypeAsync(Guid.NewGuid(), TicketTypeBuilder.UpdateRequest());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task UpdateTicketTypeAsync_WithSaleStartAfterEnd_ReturnsFail400()
    {
        // Arrange
        var id = Guid.NewGuid();
        _ticketTypeRepository.GetTicketTypeByIdAsync(id).Returns(TicketTypeBuilder.CreateEntity(id: id));

        var request = new UpdateTicketTypeRequest
        {
            Name = "Test",
            Price = 10,
            QuantityAvailable = 100,
            SaleStartDate = DateTime.UtcNow.AddDays(5),
            SaleEndTime = DateTime.UtcNow.AddDays(1), // end before start
            MinPerOrder = 1,
            MaxPerOrder = 10,
            Status = TicketTypeStatus.Active,
            SaleChannel = SaleChannel.OnlineOnly
        };

        // Act
        var result = await _sut.UpdateTicketTypeAsync(id, request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("Sale start date must be before sale end time");
    }

    [Fact]
    public async Task UpdateTicketTypeAsync_WithValidRequest_ReturnsSuccess200()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existing = TicketTypeBuilder.CreateEntity(id: id);
        _ticketTypeRepository.GetTicketTypeByIdAsync(id).Returns(existing);

        // Act
        var result = await _sut.UpdateTicketTypeAsync(id, TicketTypeBuilder.UpdateRequest(name: "Premium"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        existing.Name.Should().Be("Premium");
        await _ticketTypeRepository.Received(1).UpdateTicketTypeAsync(Arg.Any<TicketType>());
    }

    // ── ProcessTicketPurchaseAsync ─────────────────────────────────────────

    [Fact]
    public async Task ProcessTicketPurchaseAsync_WhenInsufficientStock_ReturnsFail400()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ticketType = TicketTypeBuilder.CreateEntity(id: id, quantityAvailable: 10, quantitySold: 9);
        _ticketTypeRepository.GetTicketTypeByIdAsync(id).Returns(ticketType);

        // Act
        var result = await _sut.ProcessTicketPurchaseAsync(id, 5);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("Not enough tickets available");
    }

    [Fact]
    public async Task ProcessTicketPurchaseAsync_WithSufficientStock_ReturnsSuccess200()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ticketType = TicketTypeBuilder.CreateEntity(id: id, quantityAvailable: 100, quantitySold: 0);
        _ticketTypeRepository.GetTicketTypeByIdAsync(id).Returns(ticketType);

        // Act
        var result = await _sut.ProcessTicketPurchaseAsync(id, 3);

        // Assert
        result.IsSuccess.Should().BeTrue();
        ticketType.QuantitySold.Should().Be(3);
    }

    // ── DeleteTicketTypeAsync ──────────────────────────────────────────────

    [Fact]
    public async Task DeleteTicketTypeAsync_WhenNotFound_ReturnsFail404()
    {
        // Arrange
        _ticketTypeRepository.TicketTypeExistsAsync(Arg.Any<Guid>()).Returns(false);

        // Act
        var result = await _sut.DeleteTicketTypeAsync(Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task DeleteTicketTypeAsync_WhenExists_ReturnsSuccess200()
    {
        // Arrange
        var id = Guid.NewGuid();
        _ticketTypeRepository.TicketTypeExistsAsync(id).Returns(true);
        _ticketTypeRepository.DeleteTicketTypeAsync(id).Returns(true);

        // Act
        var result = await _sut.DeleteTicketTypeAsync(id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
    }
}
