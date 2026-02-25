using EventService_Application.DTOs;
using EventService_Application.Interfaces;
using EventService_Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace EventService_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QaQuestionController : ControllerBase
    {
        private readonly IQaQuestionService _qaQuestionService;
        public QaQuestionController(IQaQuestionService qaQuestionService)
        {
            _qaQuestionService = qaQuestionService;
        }

        [HttpPost]
        public async Task<IActionResult> AddQaQuestionAsync([FromBody] CreateQaQuestionDto request)
        {
            var result = await _qaQuestionService.AddQaQuestionAsync(request);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        [HttpGet("{session:guid}")]
        public async Task<IActionResult> GetQaQuestionBySessionIdAsync(Guid sessionId)
        {
            var result = await _qaQuestionService.GetQaQuestionsBySessionIdAsync(sessionId);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }

        }

        [HttpGet("question/{userId:guid}")]
        public async Task<IActionResult> GetQaQuestionByUserIdAsync(Guid userId)
        {
            var result = await _qaQuestionService.GetQaQuestionsByUserIdAsync(userId);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        [HttpPut("{questionId:guid}")]
        public async Task<IActionResult> UpdateQaQuestionAsync(Guid questionId, [FromBody] QaStatus request)
        {
            var result = await _qaQuestionService.UpdateQaQuestionStatusAsync(questionId, request);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        [HttpDelete("{questionId:guid}")]
        public async Task<IActionResult> DeleteQaQuestionAsync(Guid questionId)
        {
            var result = await _qaQuestionService.DeleteQaQuestionAsync(questionId);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        [HttpPost("vote")]
        public async Task<IActionResult> QaQuestionVote([FromBody] QaVoteDto request)
        {
            var result = await _qaQuestionService.QaQuestionVote(request);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        [HttpGet("votes/{questionId:guid}")]
        public async Task<IActionResult> GetQaQuestionVotes(Guid questionId)
        {
            var result = await _qaQuestionService.GetQaQuestionVotes(questionId);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        [HttpGet("{questionId:guid}")]
        public async Task<IActionResult> GetQaQuestionByIdAsync(Guid questionId)
        {
            var result = await _qaQuestionService.GetQaQuestionByIdAsync(questionId);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
    }
}
