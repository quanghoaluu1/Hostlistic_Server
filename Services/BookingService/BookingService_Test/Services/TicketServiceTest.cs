using BookingService_Test.Helpers.TestDataBuilders;

namespace BookingService_Test;

public class TicketServiceTest
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IQrCodeService _qrCodeService;
    private readonly TicketService _sut;

    public TicketServiceTest()
    {
        _ticketRepository = Substitute.For<ITicketRepository>();
        _qrCodeService = Substitute.For<IQrCodeService>();
        _sut = new TicketService(_ticketRepository, _qrCodeService);
    }

    [Fact]
    public async Task CreateTicketAsync_WithValidRequest_ReturnsSuccess201AndGeneratesQr()
    {
        var ticketId = Guid.NewGuid();
        Ticket? capturedTicket = null;

        _ticketRepository
            .When(x => x.AddTicketAsync(Arg.Any<Ticket>()))
            .Do(call =>
            {
                var addedTicket = call.Arg<Ticket>()!;
                capturedTicket = addedTicket;
                addedTicket.Id = ticketId;
            });

        _ticketRepository
            .AddTicketAsync(Arg.Any<Ticket>())
            .Returns(Task.FromResult(new Ticket()));

        _qrCodeService
            .GenerateQrPayloadAsync(ticketId, Arg.Any<Guid>())
            .Returns("signed-payload");

        var request = TicketBuilder.CreateRequest(eventId: Guid.NewGuid());

        var result = await _sut.CreateTicketAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        capturedTicket.Should().NotBeNull();
        capturedTicket!.QrCodeUrl.Should().Be("signed-payload");
        capturedTicket!.TicketTypeName.Should().Be(request.TicketTypeName);
        capturedTicket!.EventName.Should().Be(request.EventName);
        await _ticketRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task GetTicketByIdAsync_WhenFound_ReturnsSuccess200()
    {
        var ticketId = Guid.NewGuid();
        _ticketRepository.GetTicketByIdAsync(ticketId).Returns(TicketBuilder.CreateEntity(id: ticketId));

        var result = await _sut.GetTicketByIdAsync(ticketId);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(ticketId);
    }

    [Fact]
    public async Task GetTicketByCodeAsync_WhenNotFound_ReturnsFail404()
    {
        _ticketRepository.GetTicketByCodeAsync("NOT-FOUND").Returns((Ticket?)null);

        var result = await _sut.GetTicketByCodeAsync("NOT-FOUND");

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetTicketsByOrderIdAsync_ReturnsSuccess200WithCollection()
    {
        var orderId = Guid.NewGuid();
        _ticketRepository.GetTicketsByOrderIdAsync(orderId).Returns(
        [
            TicketBuilder.CreateEntity(orderId: orderId),
            TicketBuilder.CreateEntity(orderId: orderId)
        ]);

        var result = await _sut.GetTicketsByOrderIdAsync(orderId);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateTicketAsync_WhenNotFound_ReturnsFail404()
    {
        _ticketRepository.GetTicketByIdAsync(Arg.Any<Guid>()).Returns((Ticket?)null);

        var result = await _sut.UpdateTicketAsync(Guid.NewGuid(), TicketBuilder.UpdateRequest());

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task UpdateTicketAsync_WhenTicketExists_UpdatesIsUsedAndReturnsSuccess200()
    {
        var ticketId = Guid.NewGuid();
        var ticket = TicketBuilder.CreateEntity(id: ticketId);
        _ticketRepository.GetTicketByIdAsync(ticketId).Returns(ticket);

        var result = await _sut.UpdateTicketAsync(ticketId, TicketBuilder.UpdateRequest(isUsed: true));

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        ticket.IsUsed.Should().BeTrue();
        await _ticketRepository.Received(1).UpdateTicketAsync(ticket);
        await _ticketRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task RegenerateAllQrCodesAsync_WithExistingTickets_UpdatesAllAndReturnsCount()
    {
        var eventA = Guid.NewGuid();
        var eventB = Guid.NewGuid();

        var tickets = new List<Ticket>
        {
            TicketBuilder.CreateEntity(eventId: eventA),
            TicketBuilder.CreateEntity(eventId: eventB)
        };

        _ticketRepository.GetAllWithOrderAsync().Returns(tickets);
        _qrCodeService
            .GenerateQrPayloadAsync(Arg.Any<Guid>(), Arg.Any<Guid>())
            .Returns(call => $"{call.ArgAt<Guid>(0)}:{call.ArgAt<Guid>(1)}");

        var result = await _sut.RegenerateAllQrCodesAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().Be(2);
        tickets.All(t => !string.IsNullOrWhiteSpace(t.QrCodeUrl)).Should().BeTrue();
        await _ticketRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task RegenerateAllQrCodesAsync_WhenNoTickets_ReturnsZero()
    {
        _ticketRepository.GetAllWithOrderAsync().Returns([]);

        var result = await _sut.RegenerateAllQrCodesAsync();

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().Be(0);
        await _ticketRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task DeleteTicketAsync_WhenDeleteFails_ReturnsFail500()
    {
        var ticketId = Guid.NewGuid();
        _ticketRepository.TicketExistsAsync(ticketId).Returns(true);
        _ticketRepository.DeleteTicketAsync(ticketId).Returns(false);

        var result = await _sut.DeleteTicketAsync(ticketId);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(500);
        await _ticketRepository.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task DeleteTicketAsync_WhenTicketNotFound_ReturnsFail404()
    {
        var ticketId = Guid.NewGuid();
        _ticketRepository.TicketExistsAsync(ticketId).Returns(false);

        var result = await _sut.DeleteTicketAsync(ticketId);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task DeleteTicketAsync_WhenDeleteSucceeds_ReturnsSuccess200()
    {
        var ticketId = Guid.NewGuid();
        _ticketRepository.TicketExistsAsync(ticketId).Returns(true);
        _ticketRepository.DeleteTicketAsync(ticketId).Returns(true);

        var result = await _sut.DeleteTicketAsync(ticketId);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        await _ticketRepository.Received(1).SaveChangesAsync();
    }
}