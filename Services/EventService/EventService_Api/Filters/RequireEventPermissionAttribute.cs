using Common;
using EventService_Api.Extensions;
using EventService_Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EventService_Api.Filters;

/// <summary>
/// Enforces a granular event permission on any organizer-facing endpoint.
/// Extracts <c>eventId</c> from route data (or action arguments) and the caller's
/// user ID from JWT claims, then delegates to <see cref="IEventAuthorizationService"/>.
///
/// Usage: <c>[RequireEventPermission(EventPermissions.EditEvent)]</c>
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class RequireEventPermissionAttribute : TypeFilterAttribute
{
    public RequireEventPermissionAttribute(string permission) : base(typeof(Filter))
    {
        Arguments = [permission];
    }

    private sealed class Filter(IEventAuthorizationService authService, string permission) : IAsyncActionFilter
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

            // ── 3. Check permission (owner always passes) ──
            var granted = await authService.HasPermissionAsync(eventId, userId, permission, ct);
            if (!granted)
            {
                context.Result = new ObjectResult(
                    ApiResponse<object>.Fail(403, "You do not have permission to perform this action."))
                    { StatusCode = 403 };
                return;
            }

            await next();
        }
    }
}
