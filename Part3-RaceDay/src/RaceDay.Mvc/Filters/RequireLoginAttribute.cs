using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Filters;

/// <summary>
/// Redirects unauthenticated users to the login page and (optionally) restricts
/// a controller or action to a single role. This keeps protected pages out of reach
/// for users who are not signed in.
/// </summary>
public class RequireLoginAttribute : ActionFilterAttribute
{
    public string? Role { get; set; }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;

        if (!SessionUser.IsAuthenticated(session))
        {
            var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
            context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl = returnUrl.ToString() });
            return;
        }

        if (Role is not null && session.GetString(SessionKeys.Role) != Role)
        {
            context.Result = new RedirectToActionResult("Denied", "Account", null);
        }
    }
}
