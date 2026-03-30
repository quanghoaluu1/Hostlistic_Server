using Common;
using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using BookingService_Domain.Entities;
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

    public async Task<ApiResponse<IEnumerable<TicketDto>>> GetTicketsByOrderIdAsync(Guid orderId)
    {
        var tickets = await _ticketRepository.GetTicketsByOrderIdAsync(orderId);
        var ticketDtos = tickets.Adapt<IEnumerable<TicketDto>>();
        return ApiResponse<IEnumerable<TicketDto>>.Success(200, "Tickets retrieved successfully", ticketDtos);
    }

    public async Task<ApiResponse<TicketDto>> CreateTicketAsync(CreateTicketRequest request)
    {
        var ticket = request.Adapt<Ticket>();

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
}