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
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetFeedbackByIdAsync(Guid id)
        {
            var result = await _feedbackService.GetFeedbackByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("event/{eventId:guid}")]
        public async Task<IActionResult> GetFeedbacksByEventIdAsync(Guid eventId)
        {
            var result = await _feedbackService.GetFeedbacksByEventIdAsync(eventId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("session/{sessionId:guid}")]
        public async Task<IActionResult> GetFeedbacksBySessionIdAsync(Guid sessionId)
        {
            var result = await _feedbackService.GetFeedbacksBySessionIdAsync(sessionId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFeedbacksAsync()
        {
            var result = await _feedbackService.GetAllFeedbacksAsync();
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateFeedbackAsync(Guid id, [FromBody] UpdateFeedbackDto request)
        {
            var result = await _feedbackService.UpdateFeedbackAsync(id, request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteFeedbackAsync(Guid id)
        {
            var result = await _feedbackService.DeleteFeedbackAsync(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}
