using Common;
using BookingService_Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace BookingService_Application.Interfaces;

public interface IPaymentMethodService
{
    Task<ApiResponse<PaymentMethodDto>> GetPaymentMethodByIdAsync(Guid paymentMethodId);
    Task<ApiResponse<IEnumerable<PaymentMethodDto>>> GetActivePaymentMethodsAsync();
    Task<ApiResponse<IEnumerable<PaymentMethodDto>>> GetAllPaymentMethodsAsync();
    Task<ApiResponse<PaymentMethodDto>> GetPaymentMethodByCodeAsync(string code);
    Task<ApiResponse<PaymentMethodDto>> CreatePaymentMethodWithIconAsync(CreatePaymentMethodRequest request, IFormFile? iconFile);
    Task<ApiResponse<PaymentMethodDto>> UpdatePaymentMethodWithIconAsync(Guid paymentMethodId, UpdatePaymentMethodRequest request, IFormFile? iconFile);
    Task<ApiResponse<bool>> DeletePaymentMethodAsync(Guid paymentMethodId);
    Task<ApiResponse<PaymentOptionsResponse>> GetPaymentOptionsAsync(GetPaymentOptionsRequest request);
}