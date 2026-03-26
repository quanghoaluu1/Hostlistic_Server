using Common;

namespace IdentityService_Application.Interfaces;

public interface IBookingServiceClient
{
    Task<ApiResponse<object>> GetUserOrdersAsync(Guid userId, string? authHeader);
    Task<ApiResponse<object>> CreateWalletAsync(Guid userId);
}
