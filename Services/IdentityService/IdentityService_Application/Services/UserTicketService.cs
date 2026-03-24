using Common;
using IdentityService_Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace IdentityService_Application.Services;

public class UserTicketService(IBookingServiceClient bookingServiceClient, IHttpContextAccessor httpContextAccessor) : IUserTicketService
{
    private string? AuthHeader => httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

    public async Task<ApiResponse<object>> GetUserOrdersAsync(Guid userId)
    {
        try
        {
            return await bookingServiceClient.GetUserOrdersAsync(userId, AuthHeader);
        }
        catch (Exception ex)
        {
            return ApiResponse<object>.Fail(500, $"Error retrieving user orders: {ex.Message}");
        }
    }

    public async Task<ApiResponse<object>> GetUserTicketsAsync(Guid userId)
    {
        var ordersResult = await GetUserOrdersAsync(userId);
        if (!ordersResult.IsSuccess) return ordersResult;
        return ApiResponse<object>.Success(200, "User tickets retrieved successfully", ordersResult.Data);
    }

    public async Task<ApiResponse<object>> GetUserTicketsWithEventDetailsAsync(Guid userId)
    {
        var ordersResult = await GetUserOrdersAsync(userId);
        if (!ordersResult.IsSuccess) return ordersResult;
        return ApiResponse<object>.Success(200, "User tickets with event details retrieved successfully", ordersResult.Data);
    }
}
