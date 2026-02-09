using Common;
using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using BookingService_Domain.Entities;
using BookingService_Domain.Interfaces;
using Mapster;

namespace BookingService_Application.Services;

public class PaymentMethodService : IPaymentMethodService
{
    private readonly IPaymentMethodRepository _paymentMethodRepository;

    public PaymentMethodService(IPaymentMethodRepository paymentMethodRepository)
    {
        _paymentMethodRepository = paymentMethodRepository;
    }

    public async Task<ApiResponse<PaymentMethodDto>> GetPaymentMethodByIdAsync(Guid paymentMethodId)
    {
        var paymentMethod = await _paymentMethodRepository.GetPaymentMethodByIdAsync(paymentMethodId);
        if (paymentMethod == null)
            return ApiResponse<PaymentMethodDto>.Fail(404, "Payment method not found");

        var paymentMethodDto = paymentMethod.Adapt<PaymentMethodDto>();
        return ApiResponse<PaymentMethodDto>.Success(200, "Payment method retrieved successfully", paymentMethodDto);
    }

    public async Task<ApiResponse<IEnumerable<PaymentMethodDto>>> GetActivePaymentMethodsAsync()
    {
        var paymentMethods = await _paymentMethodRepository.GetActivePaymentMethodsAsync();
        var paymentMethodDtos = paymentMethods.Adapt<IEnumerable<PaymentMethodDto>>();
        return ApiResponse<IEnumerable<PaymentMethodDto>>.Success(200, "Active payment methods retrieved successfully", paymentMethodDtos);
    }

    public async Task<ApiResponse<IEnumerable<PaymentMethodDto>>> GetAllPaymentMethodsAsync()
    {
        var paymentMethods = await _paymentMethodRepository.GetAllPaymentMethodsAsync();
        var paymentMethodDtos = paymentMethods.Adapt<IEnumerable<PaymentMethodDto>>();
        return ApiResponse<IEnumerable<PaymentMethodDto>>.Success(200, "Payment methods retrieved successfully", paymentMethodDtos);
    }

    public async Task<ApiResponse<PaymentMethodDto>> GetPaymentMethodByCodeAsync(string code)
    {
        var paymentMethod = await _paymentMethodRepository.GetPaymentMethodByCodeAsync(code);
        if (paymentMethod == null)
            return ApiResponse<PaymentMethodDto>.Fail(404, "Payment method not found");

        var paymentMethodDto = paymentMethod.Adapt<PaymentMethodDto>();
        return ApiResponse<PaymentMethodDto>.Success(200, "Payment method retrieved successfully", paymentMethodDto);
    }

    public async Task<ApiResponse<PaymentMethodDto>> CreatePaymentMethodAsync(CreatePaymentMethodRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return ApiResponse<PaymentMethodDto>.Fail(400, "Payment method name is required");

        if (string.IsNullOrWhiteSpace(request.Code))
            return ApiResponse<PaymentMethodDto>.Fail(400, "Payment method code is required");

        // Check if code already exists
        var existingCode = await _paymentMethodRepository.PaymentMethodCodeExistsAsync(request.Code);
        if (existingCode)
            return ApiResponse<PaymentMethodDto>.Fail(400, "Payment method code already exists");

        var paymentMethod = request.Adapt<PaymentMethod>();

        await _paymentMethodRepository.AddPaymentMethodAsync(paymentMethod);
        await _paymentMethodRepository.SaveChangesAsync();

        var paymentMethodDto = paymentMethod.Adapt<PaymentMethodDto>();
        return ApiResponse<PaymentMethodDto>.Success(201, "Payment method created successfully", paymentMethodDto);
    }

    public async Task<ApiResponse<PaymentMethodDto>> UpdatePaymentMethodAsync(Guid paymentMethodId, UpdatePaymentMethodRequest request)
    {
        var existingPaymentMethod = await _paymentMethodRepository.GetPaymentMethodByIdAsync(paymentMethodId);
        if (existingPaymentMethod == null)
            return ApiResponse<PaymentMethodDto>.Fail(404, "Payment method not found");

        if (string.IsNullOrWhiteSpace(request.Name))
            return ApiResponse<PaymentMethodDto>.Fail(400, "Payment method name is required");

        // Update properties
        existingPaymentMethod.Name = request.Name;
        existingPaymentMethod.IconUrl = request.IconUrl;
        existingPaymentMethod.FeePercentage = request.FeePercentage;
        existingPaymentMethod.FixedFee = request.FixedFee;
        existingPaymentMethod.IsActive = request.IsActive;

        await _paymentMethodRepository.UpdatePaymentMethodAsync(existingPaymentMethod);
        await _paymentMethodRepository.SaveChangesAsync();

        var paymentMethodDto = existingPaymentMethod.Adapt<PaymentMethodDto>();
        return ApiResponse<PaymentMethodDto>.Success(200, "Payment method updated successfully", paymentMethodDto);
    }

    public async Task<ApiResponse<bool>> DeletePaymentMethodAsync(Guid paymentMethodId)
    {
        var exists = await _paymentMethodRepository.PaymentMethodExistsAsync(paymentMethodId);
        if (!exists)
            return ApiResponse<bool>.Fail(404, "Payment method not found");

        var deleted = await _paymentMethodRepository.DeletePaymentMethodAsync(paymentMethodId);
        if (!deleted)
            return ApiResponse<bool>.Fail(500, "Failed to delete payment method");

        await _paymentMethodRepository.SaveChangesAsync();
        return ApiResponse<bool>.Success(200, "Payment method deleted successfully", true);
    }
}