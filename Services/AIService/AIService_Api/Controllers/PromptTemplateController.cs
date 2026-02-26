using AIService_Application.DTOs.Requests;
using AIService_Application.DTOs.Responses;
using AIService_Application.Interface;
using AIService_Domain.Enum;
using Common;
using Microsoft.AspNetCore.Mvc;

namespace AIService_Api.Controllers;

[ApiController]
[Route("api/prompt-templates")]
[Produces("application/json")]
public class PromptTemplateController(
    IPromptTemplateService promptTemplateService,
    ILogger<PromptTemplateController> logger) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PromptTemplateResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PromptTemplateResponse>>>> GetAll(
        [FromQuery] PromptCategory? category,
        CancellationToken ct)
    {
        var result = category.HasValue
            ? await promptTemplateService.GetByCategoryAsync(category.Value, ct)
            : await promptTemplateService.GetAllAsync(ct);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PromptTemplateResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PromptTemplateResponse>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await promptTemplateService.GetByIdAsync(id, ct);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    [HttpGet("key/{key}")]
    [ProducesResponseType(typeof(ApiResponse<PromptTemplateResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PromptTemplateResponse>>> GetByKey(PromptTemplateKey key, CancellationToken ct)
    {
        var result = await promptTemplateService.GetByKeyAsync(key, ct);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PromptTemplateResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<PromptTemplateResponse>>> Create(
        [FromBody] CreatePromptTemplateRequest request,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await promptTemplateService.CreateAsync(request, ct);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PromptTemplateResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PromptTemplateResponse>>> Update(
        Guid id,
        [FromBody] UpdatePromptTemplateRequest request,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await promptTemplateService.UpdateAsync(id, request, ct);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id, CancellationToken ct)
    {
        var result = await promptTemplateService.DeleteAsync(id, ct);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result);

        return Ok(result);
    }
}
