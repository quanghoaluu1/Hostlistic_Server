using BookingService_Application.DTOs;
using BookingService_Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingService_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PayoutRequestsController : ControllerBase
{
    private readonly IPayoutRequestService _payoutRequestService;

    public PayoutRequestsController(IPayoutRequestService payoutRequestService)
    {
        _payoutRequestService = payoutRequestService;
    }

    [HttpGet("{payoutRequestId:guid}")]
    public async Task<IActionResult> GetPayoutRequestById(Guid payoutRequestId)
    {
        var result = await _payoutRequestService.GetPayoutRequestByIdAsync(payoutRequestId);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("event/{eventId:guid}")]
    public async Task<IActionResult> GetPayoutRequestsByEventId(Guid eventId)
    {
        var result = await _payoutRequestService.GetPayoutRequestsByEventIdAsync(eventId);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("organizer/{organizerBankInfoId:guid}")]
    public async Task<IActionResult> GetPayoutRequestsByOrganizer(Guid organizerBankInfoId)
    {
        var result = await _payoutRequestService.GetPayoutRequestsByOrganizerAsync(organizerBankInfoId);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePayoutRequest([FromBody] CreatePayoutRequestRequest request)
    {
        var result = await _payoutRequestService.CreatePayoutRequestAsync(request);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("{payoutRequestId:guid}")]
    public async Task<IActionResult> UpdatePayoutRequest(Guid payoutRequestId, [FromBody] UpdatePayoutRequestRequest request)
    {
        var result = await _payoutRequestService.UpdatePayoutRequestAsync(payoutRequestId, request);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{payoutRequestId:guid}")]
    public async Task<IActionResult> DeletePayoutRequest(Guid payoutRequestId)
    {
        var result = await _payoutRequestService.DeletePayoutRequestAsync(payoutRequestId);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }
}