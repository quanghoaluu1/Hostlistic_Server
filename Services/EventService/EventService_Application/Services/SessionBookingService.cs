using Common;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using EventService_Domain.Entities;
using EventService_Domain.Interfaces;
using Mapster;

namespace EventService_Application.Services;

public class SessionBookingService : ISessionBookingService
{
    private readonly ISessionBookingRepository _sessionBookingRepository;
    private readonly ISessionRepository _sessionRepository;

    public SessionBookingService(ISessionBookingRepository sessionBookingRepository, ISessionRepository sessionRepository)
    {
        _sessionBookingRepository = sessionBookingRepository;
        _sessionRepository = sessionRepository;
    }

    public async Task<ApiResponse<SessionBookingDto>> GetSessionBookingByIdAsync(Guid bookingId)
    {
        var sessionBooking = await _sessionBookingRepository.GetSessionBookingByIdAsync(bookingId);
        if (sessionBooking == null)
            return ApiResponse<SessionBookingDto>.Fail(404, "Session booking not found");

        var sessionBookingDto = sessionBooking.Adapt<SessionBookingDto>();
        return ApiResponse<SessionBookingDto>.Success(200, "Session booking retrieved successfully", sessionBookingDto);
    }

    public async Task<ApiResponse<PagedResult<SessionBookingDto>>> GetSessionBookingsBySessionIdAsync(Guid sessionId, BaseQueryParams request)
    {
        var sessionBookings = await _sessionBookingRepository.GetSessionBookingsBySessionIdAsync(sessionId, request.Page, request.PageSize, request.SortBy);
        var sessionBookingDtos = sessionBookings.Adapt<List<SessionBookingDto>>();
        var result = new PagedResult<SessionBookingDto>
            (
                sessionBookingDtos,
                sessionBookings.TotalItems,
                sessionBookings.TotalPages,
                sessionBookings.PageSize
            );
        return ApiResponse<PagedResult<SessionBookingDto>>.Success(200, "Session bookings retrieved successfully", result);
    }

    public async Task<ApiResponse<PagedResult<SessionBookingDto>>> GetSessionBookingsByUserIdAsync(Guid userId, BaseQueryParams request)
    {
        var sessionBookings = await _sessionBookingRepository.GetSessionBookingsByUserIdAsync(userId, request.Page, request.PageSize, request.SortBy);
        var sessionBookingDtos = sessionBookings.Adapt<List<SessionBookingDto>>();
        var result = new PagedResult<SessionBookingDto>
            (
                sessionBookingDtos,
                sessionBookings.TotalItems,
                sessionBookings.TotalPages,
                sessionBookings.PageSize
            );
        return ApiResponse<PagedResult<SessionBookingDto>>.Success(200, "Session bookings retrieved successfully", result);
    }

    public async Task<ApiResponse<SessionBookingDto>> CreateSessionBookingAsync(CreateSessionBookingRequest request)
    {
        // Check if session exists
        var sessionExists = await _sessionRepository.SessionExistsAsync(request.SessionId);
        if (!sessionExists)
            return ApiResponse<SessionBookingDto>.Fail(404, "Session not found");

        // Check if user already has a booking for this session
        var existingBooking = await _sessionBookingRepository.UserHasBookingForSessionAsync(request.UserId, request.SessionId);
        if (existingBooking)
            return ApiResponse<SessionBookingDto>.Fail(400, "User already has a booking for this session");

        var sessionBooking = request.Adapt<SessionBooking>();
        sessionBooking.Status = EventService_Domain.Enums.BookingStatus.Confirmed;

        await _sessionBookingRepository.AddSessionBookingAsync(sessionBooking);
        await _sessionBookingRepository.SaveChangesAsync();

        var sessionBookingDto = sessionBooking.Adapt<SessionBookingDto>();
        return ApiResponse<SessionBookingDto>.Success(201, "Session booking created successfully", sessionBookingDto);
    }

    public async Task<ApiResponse<SessionBookingDto>> UpdateSessionBookingAsync(Guid bookingId, UpdateSessionBookingRequest request)
    {
        var existingBooking = await _sessionBookingRepository.GetSessionBookingByIdAsync(bookingId);
        if (existingBooking == null)
            return ApiResponse<SessionBookingDto>.Fail(404, "Session booking not found");

        // Update properties
        existingBooking.Status = request.Status;

        await _sessionBookingRepository.UpdateSessionBookingAsync(existingBooking);
        await _sessionBookingRepository.SaveChangesAsync();

        var sessionBookingDto = existingBooking.Adapt<SessionBookingDto>();
        return ApiResponse<SessionBookingDto>.Success(200, "Session booking updated successfully", sessionBookingDto);
    }

    public async Task<ApiResponse<bool>> DeleteSessionBookingAsync(Guid bookingId)
    {
        var exists = await _sessionBookingRepository.SessionBookingExistsAsync(bookingId);
        if (!exists)
            return ApiResponse<bool>.Fail(404, "Session booking not found");

        var deleted = await _sessionBookingRepository.DeleteSessionBookingAsync(bookingId);
        if (!deleted)
            return ApiResponse<bool>.Fail(500, "Failed to delete session booking");

        await _sessionBookingRepository.SaveChangesAsync();
        return ApiResponse<bool>.Success(200, "Session booking deleted successfully", true);
    }
}