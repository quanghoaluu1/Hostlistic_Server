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
    private readonly IPhotoService _photoService;

    public PayoutRequestsController(IPayoutRequestService payoutRequestService, IPhotoService photoService)
    {
        _payoutRequestService = payoutRequestService;
        _photoService = photoService;
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
    public async Task<IActionResult> CreatePayoutRequest([FromForm] CreatePayoutRequestRequest request, IFormFile proofFile)
    {
        if (proofFile == null || proofFile.Length == 0)
            return BadRequest(new { message = "Proof image file is required" });
            
        var result = await _payoutRequestService.CreatePayoutRequestWithProofAsync(request, proofFile);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPatch("proof/{payoutRequestId:guid}")]
    public async Task<IActionResult> SetPayoutRequestProof(Guid payoutRequestId, IFormFile file)
    {
        var payoutRequestEntity = await _payoutRequestService.GetPayoutRequestByIdAsync(payoutRequestId);
        if (payoutRequestEntity.Data == null) return NotFound(payoutRequestEntity);
        
        var result = await _photoService.UploadPhotoAsync(file);
        if (result.Error != null) return BadRequest(result.Error);
        
        var imageUrl = result.SecureUrl.AbsoluteUri;
        var publicId = result.PublicId;
        
        var updateRequest = new UpdatePayoutRequestRequest
        {
            Status = payoutRequestEntity.Data.Status,
            ProofImageUrl = imageUrl
        };
        
        var updateResult = await _payoutRequestService.UpdatePayoutRequestWithProofAsync(payoutRequestId, updateRequest, null);
        return Ok(updateResult);
    }

    [HttpPut("{payoutRequestId:guid}")]
    public async Task<IActionResult> UpdatePayoutRequest(Guid payoutRequestId, [FromForm] UpdatePayoutRequestRequest request, IFormFile? proofFile)
    {
        var result = await _payoutRequestService.UpdatePayoutRequestWithProofAsync(payoutRequestId, request, proofFile);
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