using Common;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using EventService_Domain.Entities;
using EventService_Domain.Enums;
using EventService_Domain.Interfaces;
using Mapster;

namespace EventService_Application.Services;

public class TicketTypeService(
    ITicketTypeRepository ticketTypeRepository,
    IUserPlanServiceClient userPlanServiceClient) : ITicketTypeService
{

    public async Task<ApiResponse<TicketTypeDto>> GetTicketTypeByIdAsync(Guid ticketTypeId)
    {
        var ticketType = await ticketTypeRepository.GetTicketTypeByIdAsync(ticketTypeId);
        if (ticketType == null)
            return ApiResponse<TicketTypeDto>.Fail(404, "Ticket type not found");

        var dto = ticketType.Adapt<TicketTypeDto>();
        return ApiResponse<TicketTypeDto>.Success(200, "Ticket type retrieved successfully", dto);
    }

    public async Task<ApiResponse<IEnumerable<TicketTypeDto>>> GetTicketTypesByEventIdAsync(Guid eventId)
    {
        var ticketTypes = await ticketTypeRepository.GetTicketTypesByEventIdAsync(eventId);
        var dtos = ticketTypes.Adapt<IEnumerable<TicketTypeDto>>();
        return ApiResponse<IEnumerable<TicketTypeDto>>.Success(200, "Ticket types retrieved successfully", dtos);
    }

    public async Task<ApiResponse<IEnumerable<TicketTypeDto>>> GetTicketTypesBySessionIdAsync(Guid sessionId)
    {
        var ticketTypes = await ticketTypeRepository.GetTicketTypesBySessionIdAsync(sessionId);
        var dtos = ticketTypes.Adapt<IEnumerable<TicketTypeDto>>();
        return ApiResponse<IEnumerable<TicketTypeDto>>.Success(200, "Ticket types retrieved successfully", dtos);
    }

    public async Task<ApiResponse<TicketTypeDto>> CreateTicketTypeAsync(CreateTicketTypeRequest request, Guid userId)
    {
        var userPlanResult = await userPlanServiceClient.GetByUserIdAsync(userId, onlyActive: true);
        var activePlan = userPlanResult.Plans.FirstOrDefault();

        if (activePlan != null && 
            activePlan.SubscriptionPlan?.Name.Equals("Free", StringComparison.OrdinalIgnoreCase) == true &&
            request.Price > 0)
        {
            return ApiResponse<TicketTypeDto>.Fail(403, "Users on the Free plan can only create free tickets. Please upgrade your plan to sell paid tickets.");
        }

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
        ticketType.UpdateStreamingBenefits(request.MaxQaQuestions, request.AllowedTrackIds);

        await ticketTypeRepository.AddTicketTypeAsync(ticketType);
        await ticketTypeRepository.SaveChangesAsync();

        var dto = ticketType.Adapt<TicketTypeDto>();
        return ApiResponse<TicketTypeDto>.Success(201, "Ticket type created successfully", dto);
    }

    public async Task<ApiResponse<TicketTypeDto>> UpdateTicketTypeAsync(Guid ticketTypeId, UpdateTicketTypeRequest request)
    {
        var existing = await ticketTypeRepository.GetTicketTypeByIdAsync(ticketTypeId);
        if (existing == null)
            return ApiResponse<TicketTypeDto>.Fail(404, "Ticket type not found");

        if (string.IsNullOrWhiteSpace(request.Name))
            return ApiResponse<TicketTypeDto>.Fail(400, "Ticket type name is required");

        if (request.Price < 0)
            return ApiResponse<TicketTypeDto>.Fail(400, "Price must be greater than or equal to 0");

        if (request.QuantityAvailable < 0)
            return ApiResponse<TicketTypeDto>.Fail(400, "Quantity available must be greater than or equal to 0");

        if (request.SaleStartDate > request.SaleEndTime)
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
        existing.UpdateStreamingBenefits(request.MaxQaQuestions, request.AllowedTrackIds);

        await ticketTypeRepository.UpdateTicketTypeAsync(existing);
        await ticketTypeRepository.SaveChangesAsync();

        var dto = existing.Adapt<TicketTypeDto>();
        return ApiResponse<TicketTypeDto>.Success(200, "Ticket type updated successfully", dto);
    }

    // Handle ticket purchase - updates both QuantityAvailable and QuantitySold
    public async Task<ApiResponse<TicketTypeDto>> ProcessTicketPurchaseAsync(Guid ticketTypeId, int quantity)
    {
        var existing = await ticketTypeRepository.GetTicketTypeByIdAsync(ticketTypeId);
        if (existing == null)
            return ApiResponse<TicketTypeDto>.Fail(404, "Ticket type not found");

        // Validate availability
        var availableQuantity = existing.QuantityAvailable - existing.QuantitySold;
        if (availableQuantity < quantity)
            return ApiResponse<TicketTypeDto>.Fail(400, $"Not enough tickets available. Available: {availableQuantity}, Requested: {quantity}");

        // Update quantities
        existing.QuantitySold += quantity;
        // Note: QuantityAvailable stays the same (total capacity), only QuantitySold increases

        await ticketTypeRepository.UpdateTicketTypeAsync(existing);
        await ticketTypeRepository.SaveChangesAsync();

        var dto = existing.Adapt<TicketTypeDto>();
        return ApiResponse<TicketTypeDto>.Success(200, "Ticket purchase processed successfully", dto);
    }

    //Multi ticket purchase processing
    public async Task<ApiResponse<bool>> ProcessBulkTicketPurchaseAsync(BulkTicketPurchaseRequest request)
    {
        try
        {
            // Validate all ticket types first
            var ticketTypes = new Dictionary<Guid, TicketType>();
            foreach (var item in request.Items)
            {
                var ticketType = await ticketTypeRepository.GetTicketTypeByIdAsync(item.TicketTypeId);
                if (ticketType == null)
                    return ApiResponse<bool>.Fail(404, $"Ticket type {item.TicketTypeId} not found");

                var availableQuantity = ticketType.QuantityAvailable - ticketType.QuantitySold;
                if (availableQuantity < item.Quantity)
                    return ApiResponse<bool>.Fail(400, $"Not enough tickets available for {ticketType.Name}. Available: {availableQuantity}, Requested: {item.Quantity}");

                ticketTypes[item.TicketTypeId] = ticketType;
            }

            // Process all purchases
            foreach (var item in request.Items)
            {
                var ticketType = ticketTypes[item.TicketTypeId];
                ticketType.QuantitySold += item.Quantity;
                await ticketTypeRepository.UpdateTicketTypeAsync(ticketType);
            }

            await ticketTypeRepository.SaveChangesAsync();
            return ApiResponse<bool>.Success(200, "Bulk ticket purchase processed successfully", true);
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.Fail(500, $"Error processing bulk ticket purchase: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteTicketTypeAsync(Guid ticketTypeId)
    {
        var exists = await ticketTypeRepository.TicketTypeExistsAsync(ticketTypeId);
        if (!exists)
            return ApiResponse<bool>.Fail(404, "Ticket type not found");

        var deleted = await ticketTypeRepository.DeleteTicketTypeAsync(ticketTypeId);
        if (!deleted)
            return ApiResponse<bool>.Fail(500, "Failed to delete ticket type");

        await ticketTypeRepository.SaveChangesAsync();
        return ApiResponse<bool>.Success(200, "Ticket type deleted successfully", true);
    }
}
