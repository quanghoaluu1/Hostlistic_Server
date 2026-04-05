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
    public async Task UpdateTicketAsync_WhenNotFound_ReturnsFail404()
    {
        _ticketRepository.GetTicketByIdAsync(Arg.Any<Guid>()).Returns((Ticket?)null);

        var result = await _sut.UpdateTicketAsync(Guid.NewGuid(), TicketBuilder.UpdateRequest());

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
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
}