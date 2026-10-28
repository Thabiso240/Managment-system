using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

public class AccountController : Controller
{
    private readonly ApiClient _api;

    public AccountController(ApiClient api) => _api = api;

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (SessionUser.IsAuthenticated(HttpContext.Session))
        {
            return RedirectToRoleHome();
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _api.LoginAsync(new LoginRequest(model.Email, model.Password));

        if (!result.Success || result.Data is null)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Your email address or password is incorrect.");
            return View(model);
        }

        SaveSession(result.Data);
        TempData["Success"] = $"Welcome back, {result.Data.User.FirstName}.";

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToRoleHome();
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (SessionUser.IsAuthenticated(HttpContext.Session))
        {
            return RedirectToRoleHome();
        }

        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _api.RegisterAsync(new RegisterRequest(
            model.FirstName, model.LastName, model.Email, model.Password, model.Role));

        if (!result.Success || result.Data is null)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "We could not create your account.");
            return View(model);
        }

        SaveSession(result.Data);
        TempData["Success"] = "Your RaceDay account has been created.";
        return RedirectToRoleHome();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        TempData["Success"] = "You have been signed out.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Denied() => View();

    private void SaveSession(AuthResponse auth)
    {
        HttpContext.Session.SetString(SessionKeys.Token, auth.Token);
        HttpContext.Session.SetString(SessionKeys.UserId, auth.User.Id.ToString());
        HttpContext.Session.SetString(SessionKeys.Role, auth.User.Role);
        HttpContext.Session.SetString(SessionKeys.Name, auth.User.FullName);
        HttpContext.Session.SetString(SessionKeys.Email, auth.User.Email);

        if (!string.IsNullOrEmpty(auth.User.ProfilePictureUrl))
        {
            HttpContext.Session.SetString(SessionKeys.Photo, auth.User.ProfilePictureUrl);
        }
    }

    private IActionResult RedirectToRoleHome()
        => SessionUser.IsOrganiser(HttpContext.Session)
            ? RedirectToAction("Dashboard", "Organiser")
            : RedirectToAction("Index", "Home");
}
