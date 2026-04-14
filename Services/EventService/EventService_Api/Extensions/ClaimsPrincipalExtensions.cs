using System.Security.Claims;

namespace EventService_Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Extracts the user's GUID from the NameIdentifier claim ("sub").
    /// Returns <see cref="Guid.Empty"/> when the claim is absent or unparseable
    /// (i.e. the user is unauthenticated).
    /// </summary>
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : Guid.Empty;
    }
}
