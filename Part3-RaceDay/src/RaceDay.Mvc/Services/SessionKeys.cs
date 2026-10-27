using Microsoft.AspNetCore.Http;

namespace RaceDay.Mvc.Services;

/// <summary>Keys used to store the authenticated user's state in the server session.</summary>
public static class SessionKeys
{
    public const string Token = "rd_token";
    public const string UserId = "rd_user_id";
    public const string Role = "rd_user_role";
    public const string Name = "rd_user_name";
    public const string Email = "rd_user_email";
    public const string Photo = "rd_user_photo";
}

/// <summary>Convenience helpers for reading the signed-in user from the session.</summary>
public static class SessionUser
{
    public static bool IsAuthenticated(ISession session)
        => !string.IsNullOrEmpty(session.GetString(SessionKeys.Token));

    public static bool IsOrganiser(ISession session)
        => session.GetString(SessionKeys.Role) == "Organiser";

    public static bool IsParticipant(ISession session)
        => session.GetString(SessionKeys.Role) == "Participant";

    public static string? Name(ISession session) => session.GetString(SessionKeys.Name);

    public static string? Photo(ISession session) => session.GetString(SessionKeys.Photo);

    public static void Clear(HttpContext context) => context.Session.Clear();
}
