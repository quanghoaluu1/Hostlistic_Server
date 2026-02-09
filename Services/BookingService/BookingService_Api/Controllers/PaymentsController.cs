using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingService_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet("{paymentId:guid}")]
    public async Task<IActionResult> GetPaymentById(Guid paymentId)
    {
        var result = await _paymentService.GetPaymentByIdAsync(paymentId);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("order/{orderId:guid}")]
    public async Task<IActionResult> GetPaymentsByOrderId(Guid orderId)
    {
        var result = await _paymentService.GetPaymentsByOrderIdAsync(orderId);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
    {
        var result = await _paymentService.CreatePaymentAsync(request);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("{paymentId:guid}")]
    public async Task<IActionResult> UpdatePayment(Guid paymentId, [FromBody] UpdatePaymentRequest request)
    {
        var result = await _paymentService.UpdatePaymentAsync(paymentId, request);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{paymentId:guid}")]
    public async Task<IActionResult> DeletePayment(Guid paymentId)
    {
        var result = await _paymentService.DeletePaymentAsync(paymentId);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }
}