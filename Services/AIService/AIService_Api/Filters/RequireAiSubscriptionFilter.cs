using System.Security.Claims;
using AIService_Application.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AIService_Api.Filters;

public class RequireAiSubscriptionFilter(IAiPlanEntitlementService entitlementService) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var userIdClaim = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            context.Result = new UnauthorizedObjectResult(new { error = "User ID not found in token." });
            return;
        }

        var result = await entitlementService.EnsureCanUseAiAsync(userId, context.HttpContext.RequestAborted);
        if (!result.Success)
        {
            context.Result = new ObjectResult(new { error = result.Message })
            {
                StatusCode = result.StatusCode
            };
            return;
        }

        await next();
    }
}
