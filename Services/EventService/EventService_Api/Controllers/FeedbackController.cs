using Common;
using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventService_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;
        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        [HttpPost]
        public async Task<IActionResult> AddFeedbackAsync([FromBody] FeedbackDto request)
        {
            var result = await _feedbackService.AddFeedbackAsync(request);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetFeedbackByIdAsync(Guid id)
        {
            var result = await _feedbackService.GetFeedbackByIdAsync(id);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result);
            return Ok(result);
        }

        [HttpGet("event/{eventId:guid}")]
        public async Task<IActionResult> GetFeedbacksByEventIdAsync(Guid eventId, [FromQuery] BaseQueryParams request)
        {
            var result = await _feedbackService.GetFeedbacksByEventIdAsync(eventId, request);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result);
            return Ok(result);
        }

        [HttpGet("session/{sessionId:guid}")]
        public async Task<IActionResult> GetFeedbacksBySessionIdAsync(Guid sessionId, [FromQuery] BaseQueryParams request)
        {
            var result = await _feedbackService.GetFeedbacksBySessionIdAsync(sessionId, request);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFeedbacksAsync([FromQuery] BaseQueryParams request)
        {
            var result = await _feedbackService.GetAllFeedback(request);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result);
            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateFeedbackAsync(Guid id, [FromBody] UpdateFeedbackDto request)
        {
            var result = await _feedbackService.UpdateFeedbackAsync(id, request);
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result);
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public IActionResult DeleteFeedbackAsync(Guid id)
        {
            var result = _feedbackService.DeleteFeedbackAsync(id).Result;
            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result);
            return Ok(result);
        }
    }
}
