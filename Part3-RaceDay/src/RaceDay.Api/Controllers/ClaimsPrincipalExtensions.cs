using System.Security.Claims;

namespace RaceDay.Api.Controllers;

/// <summary>Shared helpers for reading the authenticated user from the JWT claims.</summary>
public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : 0;
    }

    public static bool IsOrganiser(this ClaimsPrincipal principal)
        => principal.IsInRole("Organiser");
}
