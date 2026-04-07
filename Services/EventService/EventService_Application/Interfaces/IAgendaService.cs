using Common;
using EventService_Application.DTOs;

namespace EventService_Application.Interfaces;

public interface IAgendaService
{
    Task<ApiResponse<AgendaResponse>> GetAgendaAsync(Guid eventId, Guid? currentUserId);
}