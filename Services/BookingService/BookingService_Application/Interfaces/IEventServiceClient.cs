using BookingService_Application.DTOs;
using BookingService_Application.Services;

namespace BookingService_Application.Interfaces;

public interface IEventServiceClient
{
    Task<EventInfoDto?> GetEventInfoAsync(Guid eventId);
    Task<TicketTypeInfoDto?> GetTicketTypeInfoAsync(Guid ticketTypeId);
}

