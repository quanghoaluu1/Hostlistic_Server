using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingService_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentMethodsController : ControllerBase
{
    private readonly IPaymentMethodService _paymentMethodService;

    public PaymentMethodsController(IPaymentMethodService paymentMethodService)
    {
        _paymentMethodService = paymentMethodService;
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
    public async Task<IActionResult> CreatePaymentMethod([FromBody] CreatePaymentMethodRequest request)
    {
        var result = await _paymentMethodService.CreatePaymentMethodAsync(request);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("{paymentMethodId:guid}")]
    public async Task<IActionResult> UpdatePaymentMethod(Guid paymentMethodId, [FromBody] UpdatePaymentMethodRequest request)
    {
        var result = await _paymentMethodService.UpdatePaymentMethodAsync(paymentMethodId, request);
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