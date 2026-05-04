using BookingService_Application.DTOs;
using BookingService_Application.DTOs.PayOs;
using Common;

namespace BookingService_Application.Interfaces;

public interface ISubscriptionPurchaseService
{
    Task<ApiResponse<PurchaseSubscriptionWithWalletResponse>> PurchaseWithWalletAsync(PurchaseSubscriptionWithWalletRequest request);
    Task<ApiResponse<PayOsCheckoutResponse>> PurchaseWithPayOsAsync(PurchaseSubscriptionWithPayOsRequest request);
}
