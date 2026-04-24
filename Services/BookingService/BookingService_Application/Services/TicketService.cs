using Common;
using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using BookingService_Domain.Entities;
using BookingService_Domain.Enum;
using BookingService_Domain.Interfaces;
using Mapster;

namespace BookingService_Application.Services;

public class TicketService : ITicketService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IQrCodeService _qrCodeService;

    public TicketService(ITicketRepository ticketRepository, IQrCodeService qrCodeService)
    {
        _ticketRepository = ticketRepository;
        _qrCodeService = qrCodeService;
    }

    public async Task<ApiResponse<TicketDto>> GetTicketByIdAsync(Guid ticketId)
    {
        var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);
        if (ticket == null)
            return ApiResponse<TicketDto>.Fail(404, "Ticket not found");

        var ticketDto = ticket.Adapt<TicketDto>();
        return ApiResponse<TicketDto>.Success(200, "Ticket retrieved successfully", ticketDto);
    }

    public async Task<ApiResponse<TicketDto>> GetTicketByCodeAsync(string ticketCode)
    {
        var ticket = await _ticketRepository.GetTicketByCodeAsync(ticketCode);
        if (ticket == null)
            return ApiResponse<TicketDto>.Fail(404, "Ticket not found");

        var ticketDto = ticket.Adapt<TicketDto>();
        return ApiResponse<TicketDto>.Success(200, "Ticket retrieved successfully", ticketDto);
    }

    public async Task<ApiResponse<bool>> CheckStreamAccessAsync(Guid eventId, Guid userId)
    {
        if (eventId == Guid.Empty)
            return ApiResponse<bool>.Fail(400, "Event id is required.");

        if (userId == Guid.Empty)
            return ApiResponse<bool>.Fail(400, "User id is required.");

        var hasAccess = await _ticketRepository.HasConfirmedAccessToEventAsync(eventId, userId);
        return ApiResponse<bool>.Success(200, "Stream access evaluated successfully.", hasAccess);
    }

    public async Task<ApiResponse<GuestLiveAccessTicketDto>> ValidateGuestLiveAccessAsync(ValidateGuestLiveAccessRequest request)
    {
        if (request.EventId == Guid.Empty)
            return ApiResponse<GuestLiveAccessTicketDto>.Fail(400, "Event id is required.");

        if (string.IsNullOrWhiteSpace(request.TicketCode))
            return ApiResponse<GuestLiveAccessTicketDto>.Fail(400, "Ticket code is required.");

        var normalizedTicketCode = NormalizeTicketCode(request.TicketCode);
        var ticket = await _ticketRepository.GetTicketByCodeAsync(normalizedTicketCode);
        if (ticket == null)
            return ApiResponse<GuestLiveAccessTicketDto>.Fail(404, "Ticket not found.");

        if (ticket.Order.EventId != request.EventId)
            return ApiResponse<GuestLiveAccessTicketDto>.Fail(403, "This ticket does not belong to the requested event.");

        if (ticket.Order.Status != OrderStatus.Confirmed)
            return ApiResponse<GuestLiveAccessTicketDto>.Fail(403, "This ticket is not eligible for live access.");

        var response = new GuestLiveAccessTicketDto
        {
            TicketId = ticket.Id,
            EventId = ticket.Order.EventId,
            OrderId = ticket.OrderId,
            TicketCode = ticket.TicketCode,
            HolderName = ticket.HolderName,
            HolderEmail = ticket.HolderEmail,
            IsUsed = ticket.IsUsed
        };

        return ApiResponse<GuestLiveAccessTicketDto>.Success(200, "Ticket is valid for guest live access.", response);
    }

    public async Task<ApiResponse<IEnumerable<TicketDto>>> GetTicketsByOrderIdAsync(Guid orderId)
    {
        var tickets = await _ticketRepository.GetTicketsByOrderIdAsync(orderId);
        var ticketDtos = tickets.Adapt<IEnumerable<TicketDto>>();
        return ApiResponse<IEnumerable<TicketDto>>.Success(200, "Tickets retrieved successfully", ticketDtos);
    }

    public async Task<ApiResponse<TicketDto>> CreateTicketAsync(CreateTicketRequest request)
    {
        var ticket = request.Adapt<Ticket>();
        ticket.TicketTypeName = request.TicketTypeName;
        ticket.EventName = request.EventName;
        ticket.HolderName = request.HolderName;
        ticket.HolderEmail = request.HolderEmail;
        ticket.HolderPhone = request.HolderPhone;

        await _ticketRepository.AddTicketAsync(ticket); // sets ticket.Id and ticket.TicketCode
        ticket.QrCodeUrl = await _qrCodeService.GenerateQrPayloadAsync(ticket.Id, request.EventId);
        await _ticketRepository.SaveChangesAsync();

        var ticketDto = ticket.Adapt<TicketDto>();
        return ApiResponse<TicketDto>.Success(201, "Ticket created successfully", ticketDto);
    }

    public async Task<ApiResponse<TicketDto>> UpdateTicketAsync(Guid ticketId, UpdateTicketRequest request)
    {
        var existingTicket = await _ticketRepository.GetTicketByIdAsync(ticketId);
        if (existingTicket == null)
            return ApiResponse<TicketDto>.Fail(404, "Ticket not found");

        // Update properties
        existingTicket.IsUsed = request.IsUsed;

        await _ticketRepository.UpdateTicketAsync(existingTicket);
        await _ticketRepository.SaveChangesAsync();

        var ticketDto = existingTicket.Adapt<TicketDto>();
        return ApiResponse<TicketDto>.Success(200, "Ticket updated successfully", ticketDto);
    }

    public async Task<ApiResponse<int>> RegenerateAllQrCodesAsync()
    {
        var tickets = (await _ticketRepository.GetAllWithOrderAsync()).ToList();
        foreach (var ticket in tickets)
            ticket.QrCodeUrl = await _qrCodeService.GenerateQrPayloadAsync(ticket.Id, ticket.Order.EventId);

        await _ticketRepository.SaveChangesAsync();
        return ApiResponse<int>.Success(200, "QR codes regenerated", tickets.Count);
    }

    public async Task<ApiResponse<bool>> DeleteTicketAsync(Guid ticketId)
    {
        var exists = await _ticketRepository.TicketExistsAsync(ticketId);
        if (!exists)
            return ApiResponse<bool>.Fail(404, "Ticket not found");

        var deleted = await _ticketRepository.DeleteTicketAsync(ticketId);
        if (!deleted)
            return ApiResponse<bool>.Fail(500, "Failed to delete ticket");

        await _ticketRepository.SaveChangesAsync();
        return ApiResponse<bool>.Success(200, "Ticket deleted successfully", true);
    }

    private static string NormalizeTicketCode(string? value)
    {
        return (value ?? string.Empty)
            .Trim()
            .ToUpperInvariant()
            .Replace("-", "")
            .Replace(" ", "");
    }
}
