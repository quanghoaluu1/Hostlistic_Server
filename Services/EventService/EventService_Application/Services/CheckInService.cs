using Common;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using EventService_Domain.Entities;
using EventService_Domain.Interfaces;
using Mapster;

namespace EventService_Application.Services;

public class CheckInService : ICheckInService
{
    private readonly ICheckInRepository _checkInRepository;

    public CheckInService(ICheckInRepository checkInRepository)
    {
        _checkInRepository = checkInRepository;
    }

    public async Task<ApiResponse<CheckInDto>> GetCheckInByIdAsync(Guid checkInId)
    {
        var checkIn = await _checkInRepository.GetCheckInByIdAsync(checkInId);
        if (checkIn == null)
            return ApiResponse<CheckInDto>.Fail(404, "Check-in not found");

        var dto = checkIn.Adapt<CheckInDto>();
        return ApiResponse<CheckInDto>.Success(200, "Check-in retrieved successfully", dto);
    }

    public async Task<ApiResponse<IEnumerable<CheckInDto>>> GetCheckInsByEventIdAsync(Guid eventId)
    {
        var checkIns = await _checkInRepository.GetCheckInsByEventIdAsync(eventId);
        var dtos = checkIns.Adapt<IEnumerable<CheckInDto>>();
        return ApiResponse<IEnumerable<CheckInDto>>.Success(200, "Check-ins retrieved successfully", dtos);
    }

    public async Task<ApiResponse<IEnumerable<CheckInDto>>> GetCheckInsBySessionIdAsync(Guid sessionId)
    {
        var checkIns = await _checkInRepository.GetCheckInsBySessionIdAsync(sessionId);
        var dtos = checkIns.Adapt<IEnumerable<CheckInDto>>();
        return ApiResponse<IEnumerable<CheckInDto>>.Success(200, "Check-ins retrieved successfully", dtos);
    }

    public async Task<ApiResponse<CheckInDto?>> GetCheckInByTicketIdAsync(Guid ticketId)
    {
        var checkIn = await _checkInRepository.GetCheckInByTicketIdAsync(ticketId);
        if (checkIn == null)
            return ApiResponse<CheckInDto?>.Success(200, "No check-in found for this ticket", null);

        var dto = checkIn.Adapt<CheckInDto>();
        return ApiResponse<CheckInDto?>.Success(200, "Check-in retrieved successfully", dto);
    }

    public async Task<ApiResponse<CheckInDto>> CreateCheckInAsync(Guid checkedByUserId, CreateCheckInRequest request)
    {
        // Check if this ticket has already been checked in
        var existingCheckIn = await _checkInRepository.GetCheckInByTicketIdAsync(request.TicketId);
        if (existingCheckIn != null)
            return ApiResponse<CheckInDto>.Fail(400, "This ticket has already been checked in");

        var checkIn = request.Adapt<CheckIn>();
        checkIn.CheckedBy = checkedByUserId;
        checkIn.CheckedInAt = DateTime.UtcNow;

        await _checkInRepository.AddCheckInAsync(checkIn);
        await _checkInRepository.SaveChangesAsync();

        var dto = checkIn.Adapt<CheckInDto>();
        return ApiResponse<CheckInDto>.Success(201, "Check-in created successfully", dto);
    }

    public async Task<ApiResponse<CheckInDto>> UpdateCheckInAsync(Guid checkInId, UpdateCheckInRequest request)
    {
        var existing = await _checkInRepository.GetCheckInByIdAsync(checkInId);
        if (existing == null)
            return ApiResponse<CheckInDto>.Fail(404, "Check-in not found");

        existing.CheckInLocation = request.CheckInLocation;
        existing.CheckInType = request.CheckInType;

        await _checkInRepository.UpdateCheckInAsync(existing);
        await _checkInRepository.SaveChangesAsync();

        var dto = existing.Adapt<CheckInDto>();
        return ApiResponse<CheckInDto>.Success(200, "Check-in updated successfully", dto);
    }

    public async Task<ApiResponse<bool>> DeleteCheckInAsync(Guid checkInId)
    {
        var exists = await _checkInRepository.CheckInExistsAsync(checkInId);
        if (!exists)
            return ApiResponse<bool>.Fail(404, "Check-in not found");

        var deleted = await _checkInRepository.DeleteCheckInAsync(checkInId);
        if (!deleted)
            return ApiResponse<bool>.Fail(500, "Failed to delete check-in");

        await _checkInRepository.SaveChangesAsync();
        return ApiResponse<bool>.Success(200, "Check-in deleted successfully", true);
    }
}
