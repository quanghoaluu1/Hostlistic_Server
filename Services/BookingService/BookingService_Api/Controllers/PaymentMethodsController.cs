using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingService_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class PaymentMethodsController : ControllerBase
{
    private readonly IPaymentMethodService _paymentMethodService;
    private readonly IPhotoService _photoService;

    public PaymentMethodsController(IPaymentMethodService paymentMethodService, IPhotoService photoService)
    {
        _paymentMethodService = paymentMethodService;
        _photoService = photoService;
    }

    [HttpGet("{paymentMethodId:guid}")]
    public async Task<IActionResult> GetPaymentMethodById(Guid paymentMethodId)
    {
        var result = await _paymentMethodService.GetPaymentMethodByIdAsync(paymentMethodId);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActivePaymentMethods()
    {
        var result = await _paymentMethodService.GetActivePaymentMethodsAsync();
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPaymentMethods()
    {
        var result = await _paymentMethodService.GetAllPaymentMethodsAsync();
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("code/{code}")]
    public async Task<IActionResult> GetPaymentMethodByCode(string code)
    {
        var result = await _paymentMethodService.GetPaymentMethodByCodeAsync(code);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePaymentMethod([FromForm] CreatePaymentMethodRequest request, IFormFile iconFile)
    {
        if (iconFile == null || iconFile.Length == 0)
            return BadRequest(new { message = "Icon file is required" });
            
        var result = await _paymentMethodService.CreatePaymentMethodWithIconAsync(request, iconFile);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPatch("icon/{paymentMethodId:guid}")]
    public async Task<IActionResult> SetPaymentMethodIcon(Guid paymentMethodId, IFormFile file)
    {
        var paymentMethodEntity = await _paymentMethodService.GetPaymentMethodByIdAsync(paymentMethodId);
        if (paymentMethodEntity.Data == null) return NotFound(paymentMethodEntity);
        
        var result = await _photoService.UploadPhotoAsync(file);
        if (result.Error != null) return BadRequest(result.Error);
        
        var imageUrl = result.SecureUrl.AbsoluteUri;
        var publicId = result.PublicId;
        
        var updateRequest = new UpdatePaymentMethodRequest
        {
            Name = paymentMethodEntity.Data.Name,
            IconUrl = imageUrl,
            FeePercentage = paymentMethodEntity.Data.FeePercentage,
            FixedFee = paymentMethodEntity.Data.FixedFee,
            IsActive = paymentMethodEntity.Data.IsActive
        };
        
        var updateResult = await _paymentMethodService.UpdatePaymentMethodWithIconAsync(paymentMethodId, updateRequest, null);
        return Ok(updateResult);
    }

    [HttpPut("{paymentMethodId:guid}")]
    public async Task<IActionResult> UpdatePaymentMethod(Guid paymentMethodId, [FromForm] UpdatePaymentMethodRequest request, IFormFile? iconFile)
    {
        var result = await _paymentMethodService.UpdatePaymentMethodWithIconAsync(paymentMethodId, request, iconFile);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{paymentMethodId:guid}")]
    public async Task<IActionResult> DeletePaymentMethod(Guid paymentMethodId)
    {
        var result = await _paymentMethodService.DeletePaymentMethodAsync(paymentMethodId);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }
}