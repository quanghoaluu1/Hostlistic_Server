using BookingService_Application.DTOs;
using Common;

namespace BookingService_Application.Interfaces;

public interface IAttendeeService
{
    Task<ApiResponse<AttendeeListResponse>> GetAttendeesAsync(Guid eventId, AttendeeListRequest request, CancellationToken ct = default);
    Task<ApiResponse<AttendeeSummaryDto>> GetAttendeeSummaryAsync(Guid eventId, CancellationToken ct = default);
}
