using BookingService_Application.DTOs;
using Common;

namespace BookingService_Application.Interfaces;

public interface ISubscriptionPurchaseService
{
    Task<ApiResponse<PurchaseSubscriptionWithWalletResponse>> PurchaseWithWalletAsync(PurchaseSubscriptionWithWalletRequest request);
}
