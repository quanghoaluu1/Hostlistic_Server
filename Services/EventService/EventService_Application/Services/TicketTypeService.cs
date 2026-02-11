using Common;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using EventService_Domain.Entities;
using EventService_Domain.Enums;
using EventService_Domain.Interfaces;
using Mapster;

namespace EventService_Application.Services;

public class TicketTypeService : ITicketTypeService
{
    private readonly ITicketTypeRepository _ticketTypeRepository;

    public TicketTypeService(ITicketTypeRepository ticketTypeRepository)
    {
        _ticketTypeRepository = ticketTypeRepository;
    }

    public async Task<ApiResponse<TicketTypeDto>> GetTicketTypeByIdAsync(Guid ticketTypeId)
    {
        var ticketType = await _ticketTypeRepository.GetTicketTypeByIdAsync(ticketTypeId);
        if (ticketType == null)
            return ApiResponse<TicketTypeDto>.Fail(404, "Ticket type not found");

        var dto = ticketType.Adapt<TicketTypeDto>();
        return ApiResponse<TicketTypeDto>.Success(200, "Ticket type retrieved successfully", dto);
    }

    public async Task<ApiResponse<IEnumerable<TicketTypeDto>>> GetTicketTypesByEventIdAsync(Guid eventId)
    {
        var ticketTypes = await _ticketTypeRepository.GetTicketTypesByEventIdAsync(eventId);
        var dtos = ticketTypes.Adapt<IEnumerable<TicketTypeDto>>();
        return ApiResponse<IEnumerable<TicketTypeDto>>.Success(200, "Ticket types retrieved successfully", dtos);
    }

    public async Task<ApiResponse<IEnumerable<TicketTypeDto>>> GetTicketTypesBySessionIdAsync(Guid sessionId)
    {
        var ticketTypes = await _ticketTypeRepository.GetTicketTypesBySessionIdAsync(sessionId);
        var dtos = ticketTypes.Adapt<IEnumerable<TicketTypeDto>>();
        return ApiResponse<IEnumerable<TicketTypeDto>>.Success(200, "Ticket types retrieved successfully", dtos);
    }

    public async Task<ApiResponse<TicketTypeDto>> CreateTicketTypeAsync(CreateTicketTypeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return ApiResponse<TicketTypeDto>.Fail(400, "Ticket type name is required");

        if (request.Price < 0)
            return ApiResponse<TicketTypeDto>.Fail(400, "Price must be greater than or equal to 0");

        if (request.QuantityAvailable <= 0)
            return ApiResponse<TicketTypeDto>.Fail(400, "Quantity available must be greater than 0");

        if (request.MinPerOrder < 0 || request.MaxPerOrder < 0 || request.MinPerOrder > request.MaxPerOrder)
            return ApiResponse<TicketTypeDto>.Fail(400, "Min per order and max per order must be valid (0 <= min <= max)");

        var ticketType = request.Adapt<TicketType>();
        ticketType.QuantitySold = 0;
        ticketType.Status = TicketTypeStatus.Active;
        ticketType.SaleStartDate = DateTime.SpecifyKind(request.SaleStartDate, DateTimeKind.Utc);
        ticketType.SaleEndTime = DateTime.SpecifyKind(request.SaleEndTime, DateTimeKind.Utc);

        await _ticketTypeRepository.AddTicketTypeAsync(ticketType);
        await _ticketTypeRepository.SaveChangesAsync();

        var dto = ticketType.Adapt<TicketTypeDto>();
        return ApiResponse<TicketTypeDto>.Success(201, "Ticket type created successfully", dto);
    }

    public async Task<ApiResponse<TicketTypeDto>> UpdateTicketTypeAsync(Guid ticketTypeId, UpdateTicketTypeRequest request)
    {
        var existing = await _ticketTypeRepository.GetTicketTypeByIdAsync(ticketTypeId);
        if (existing == null)
            return ApiResponse<TicketTypeDto>.Fail(404, "Ticket type not found");

        if (string.IsNullOrWhiteSpace(request.Name))
            return ApiResponse<TicketTypeDto>.Fail(400, "Ticket type name is required");

        if (request.Price < 0)
            return ApiResponse<TicketTypeDto>.Fail(400, "Price must be greater than or equal to 0");

        if (request.QuantityAvailable < 0)
            return ApiResponse<TicketTypeDto>.Fail(400, "Quantity available must be greater than or equal to 0");

        if (request.SaleStartDate >= request.SaleEndTime)
            return ApiResponse<TicketTypeDto>.Fail(400, "Sale start date must be before sale end time");

        if (request.MinPerOrder < 0 || request.MaxPerOrder < 0 || request.MinPerOrder > request.MaxPerOrder)
            return ApiResponse<TicketTypeDto>.Fail(400, "Min per order and max per order must be valid (0 <= min <= max)");

        existing.Name = request.Name;
        existing.Price = request.Price;
        existing.Description = request.Description;
        existing.QuantityAvailable = request.QuantityAvailable;
        existing.SaleStartDate = DateTime.SpecifyKind(request.SaleStartDate, DateTimeKind.Utc);
        existing.SaleEndTime = DateTime.SpecifyKind(request.SaleEndTime, DateTimeKind.Utc);
        existing.MinPerOrder = request.MinPerOrder;
        existing.MaxPerOrder = request.MaxPerOrder;
        existing.IsRequireHolderInfo = request.IsRequireHolderInfo;
        existing.Status = request.Status;
        existing.SaleChannel = request.SaleChannel;

        await _ticketTypeRepository.UpdateTicketTypeAsync(existing);
        await _ticketTypeRepository.SaveChangesAsync();

        var dto = existing.Adapt<TicketTypeDto>();
        return ApiResponse<TicketTypeDto>.Success(200, "Ticket type updated successfully", dto);
    }

    public async Task<ApiResponse<bool>> DeleteTicketTypeAsync(Guid ticketTypeId)
    {
        var exists = await _ticketTypeRepository.TicketTypeExistsAsync(ticketTypeId);
        if (!exists)
            return ApiResponse<bool>.Fail(404, "Ticket type not found");

        var deleted = await _ticketTypeRepository.DeleteTicketTypeAsync(ticketTypeId);
        if (!deleted)
            return ApiResponse<bool>.Fail(500, "Failed to delete ticket type");

        await _ticketTypeRepository.SaveChangesAsync();
        return ApiResponse<bool>.Success(200, "Ticket type deleted successfully", true);
    }
}
