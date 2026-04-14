using Common;
using EventService_Api.Extensions;
using EventService_Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EventService_Api.Filters;

/// <summary>
/// Restricts an endpoint to the event owner (the user whose ID matches
/// <c>Event.OrganizerId</c>). Use for destructive or ownership-transfer operations.
///
/// Usage: <c>[RequireEventOwner]</c>
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class RequireEventOwnerAttribute : TypeFilterAttribute
{
    public RequireEventOwnerAttribute() : base(typeof(Filter)) { }

    private sealed class Filter(IEventAuthorizationService authService) : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var ct = context.HttpContext.RequestAborted;

            // ── 1. Resolve eventId from route, then action arguments ──
            Guid eventId = Guid.Empty;

            if (context.RouteData.Values.TryGetValue("eventId", out var routeValue))
                Guid.TryParse(routeValue?.ToString(), out eventId);

            if (eventId == Guid.Empty
                && context.ActionArguments.TryGetValue("eventId", out var argValue)
                && argValue is Guid guidArg)
            {
                eventId = guidArg;
            }

            if (eventId == Guid.Empty)
            {
                context.Result = new ObjectResult(ApiResponse<object>.Fail(400, "Event ID is required."))
                    { StatusCode = 400 };
                return;
            }

            // ── 2. Resolve caller identity ──
            var userId = context.HttpContext.User.GetUserId();
            if (userId == Guid.Empty)
            {
                context.Result = new ObjectResult(ApiResponse<object>.Fail(401, "Unauthorized."))
                    { StatusCode = 401 };
                return;
            }

            // ── 3. Ownership check ──
            var isOwner = await authService.IsEventOwnerAsync(eventId, userId, ct);
            if (!isOwner)
            {
                context.Result = new ObjectResult(
                    ApiResponse<object>.Fail(403, "Only the event owner can perform this action."))
                    { StatusCode = 403 };
                return;
            }

            await next();
        }
    }
}
