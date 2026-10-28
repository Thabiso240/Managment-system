using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Filters;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

/// <summary>The signed-in user's profile, including profile picture upload.</summary>
[RequireLogin]
public class ProfileController : Controller
{
    private static readonly string[] AllowedImageTypes = { "image/jpeg", "image/png", "image/webp", "image/gif" };

    private readonly ApiClient _api;

    public ProfileController(ApiClient api) => _api = api;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _api.GetProfileAsync();
        if (user is null)
        {
            TempData["Error"] = "We could not load your profile.";
            return RedirectToAction("Index", "Home");
        }

        return View(new ProfileViewModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            DateOfBirth = user.DateOfBirth,
            Gender = user.Gender,
            City = user.City,
            Bio = user.Bio,
            Role = user.Role,
            ProfilePictureUrl = user.ProfilePictureUrl
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(ProfileViewModel model)
    {
        if (!ModelState.IsValid) return View("Index", model);

        var result = await _api.UpdateProfileAsync(new UpdateProfileRequest(
            model.FirstName, model.LastName, model.Phone, model.DateOfBirth, model.Gender, model.City, model.Bio));

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "We could not save your profile.");
            return View("Index", model);
        }

        HttpContext.Session.SetString(SessionKeys.Name, result.Data!.FullName);
        TempData["Success"] = "Your profile has been updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadPhoto(IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            TempData["Error"] = "Please choose an image file to upload.";
            return RedirectToAction(nameof(Index));
        }

        if (file.Length > 5 * 1024 * 1024)
        {
            TempData["Error"] = "The image must be 5 MB or smaller.";
            return RedirectToAction(nameof(Index));
        }

        if (!AllowedImageTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            TempData["Error"] = "Only JPG, PNG, WEBP or GIF images are supported.";
            return RedirectToAction(nameof(Index));
        }

        await using var stream = file.OpenReadStream();
        var result = await _api.UploadProfilePhotoAsync(stream, file.FileName, file.ContentType);

        if (!result.Success)
        {
            TempData["Error"] = result.Error ?? "We could not upload your picture.";
            return RedirectToAction(nameof(Index));
        }

        HttpContext.Session.SetString(SessionKeys.Photo, result.Data!.ProfilePictureUrl ?? string.Empty);
        TempData["Success"] = "Your profile picture has been updated.";
        return RedirectToAction(nameof(Index));
    }
}
