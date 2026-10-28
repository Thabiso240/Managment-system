using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Filters;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

/// <summary>
/// The Organiser's workspace: dashboard, event management, category management,
/// enrolments and results entry. Every action is restricted to the Organiser role.
/// </summary>
[RequireLogin(Role = "Organiser")]
public class OrganiserController : Controller
{
    private static readonly string[] AllowedImageTypes = { "image/jpeg", "image/png", "image/webp", "image/gif" };

    private readonly ApiClient _api;

    public OrganiserController(ApiClient api) => _api = api;

    // -----------------------------------------------------------------------
    // Dashboard
    // -----------------------------------------------------------------------

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var dashboard = await _api.GetDashboardAsync();

        if (dashboard is null)
        {
            TempData["Error"] = "We could not load your dashboard.";
            return View(new OrganiserDashboardDto(0, 0, 0, new List<EventSummaryDto>()));
        }

        return View(dashboard);
    }

    // -----------------------------------------------------------------------
    // Event management
    // -----------------------------------------------------------------------

    [HttpGet]
    public async Task<IActionResult> Events()
    {
        var events = await _api.GetMyEventsAsync();
        return View(events);
    }

    [HttpGet]
    public IActionResult CreateEvent() => View("EventForm", new EventFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEvent(EventFormViewModel model, IFormFile? banner)
    {
        if (!ModelState.IsValid) return View("EventForm", model);

        var result = await _api.CreateEventAsync(new CreateEventRequest(
            model.Name, model.Description, model.Date, model.Location, model.DistanceKm, model.EventType));

        if (!result.Success || result.Data is null)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "We could not create the event.");
            return View("EventForm", model);
        }

        if (banner is not null && banner.Length > 0)
        {
            await TryUploadBannerAsync(result.Data.Id, banner);
        }

        TempData["Success"] = $"Event '{result.Data.Name}' has been created. Add categories next.";
        return RedirectToAction(nameof(Categories), new { id = result.Data.Id });
    }

    [HttpGet]
    public async Task<IActionResult> EditEvent(int id)
    {
        var ev = await _api.GetEventAsync(id);
        if (ev is null) return NotFound();

        return View("EventForm", new EventFormViewModel
        {
            Id = ev.Id,
            Name = ev.Name,
            Description = ev.Description,
            Date = ev.Date,
            Location = ev.Location,
            DistanceKm = ev.DistanceKm,
            EventType = ev.EventType,
            BannerImageUrl = ev.BannerImageUrl
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditEvent(EventFormViewModel model, IFormFile? banner)
    {
        if (!ModelState.IsValid) return View("EventForm", model);

        var result = await _api.UpdateEventAsync(model.Id, new UpdateEventRequest(
            model.Name, model.Description, model.Date, model.Location, model.DistanceKm, model.EventType));

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "We could not update the event.");
            return View("EventForm", model);
        }

        if (banner is not null && banner.Length > 0)
        {
            await TryUploadBannerAsync(model.Id, banner);
        }

        TempData["Success"] = $"Event '{model.Name}' has been updated.";
        return RedirectToAction(nameof(Events));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var result = await _api.DeleteEventAsync(id);

        TempData[result.Success ? "Success" : "Error"] =
            result.Success ? "The event has been deleted." : result.Error ?? "We could not delete the event.";

        return RedirectToAction(nameof(Events));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadBanner(int id, IFormFile banner)
    {
        var error = await TryUploadBannerAsync(id, banner);
        TempData[error ?? "Success"] = error ?? "The event banner has been updated.";
        return RedirectToAction(nameof(EditEvent), new { id });
    }

    // -----------------------------------------------------------------------
    // Category management
    // -----------------------------------------------------------------------

    [HttpGet]
    public async Task<IActionResult> Categories(int id)
    {
        var ev = await _api.GetEventAsync(id);
        if (ev is null) return NotFound();

        ViewBag.Event = ev;
        ViewBag.NewCategory = new CategoryFormViewModel { EventId = ev.Id };
        return View(ev.Categories);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCategory(CategoryFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please provide a valid category name.";
            return RedirectToAction(nameof(Categories), new { id = model.EventId });
        }

        var result = await _api.CreateCategoryAsync(model.EventId, new CreateCategoryRequest(
            model.Name, model.AgeGroup, model.DistanceKm, model.EntryFee, model.MaxEntries));

        TempData[result.Success ? "Success" : "Error"] =
            result.Success ? "Category added." : result.Error ?? "We could not add the category.";

        return RedirectToAction(nameof(Categories), new { id = model.EventId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCategory(CategoryFormViewModel model)
    {
        var result = await _api.UpdateCategoryAsync(model.Id, new UpdateCategoryRequest(
            model.Name, model.AgeGroup, model.DistanceKm, model.EntryFee, model.MaxEntries));

        TempData[result.Success ? "Success" : "Error"] =
            result.Success ? "Category updated." : result.Error ?? "We could not update the category.";

        return RedirectToAction(nameof(Categories), new { id = model.EventId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id, int eventId)
    {
        var result = await _api.DeleteCategoryAsync(id);

        TempData[result.Success ? "Success" : "Error"] =
            result.Success ? "Category removed." : result.Error ?? "We could not remove the category.";

        return RedirectToAction(nameof(Categories), new { id = eventId });
    }

    // -----------------------------------------------------------------------
    // Enrolments
    // -----------------------------------------------------------------------

    [HttpGet]
    public async Task<IActionResult> Enrolments(int id)
    {
        var ev = await _api.GetEventAsync(id);
        if (ev is null) return NotFound();

        var enrolments = await _api.GetEventEnrolmentsAsync(id);

        ViewBag.Event = ev;
        return View(enrolments);
    }

    // -----------------------------------------------------------------------
    // Results
    // -----------------------------------------------------------------------

    [HttpGet]
    public async Task<IActionResult> Results(int id)
    {
        var ev = await _api.GetEventAsync(id);
        if (ev is null) return NotFound();

        var enrolments = await _api.GetEventEnrolmentsAsync(id);

        ViewBag.Event = ev;
        ViewBag.PublishedCount = enrolments.Count(e => e.ResultPublished);
        return View(enrolments);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordResult(int eventId, int enrolmentId, string? finishTime, int position)
    {
        if (string.IsNullOrWhiteSpace(finishTime))
        {
            TempData["Error"] = "Please enter a finish time in hh:mm:ss format.";
            return RedirectToAction(nameof(Results), new { id = eventId });
        }

        var result = await _api.RecordResultAsync(eventId, new RecordResultRequest(enrolmentId, finishTime, position));

        TempData[result.Success ? "Success" : "Error"] =
            result.Success ? "Result saved." : result.Error ?? "We could not save that result.";

        return RedirectToAction(nameof(Results), new { id = eventId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PublishResults(int eventId, int totalFinishers)
    {
        var result = await _api.PublishResultsAsync(eventId, totalFinishers);

        TempData[result.Success ? "Success" : "Error"] =
            result.Success ? "Results published. Participants can now view their race history." : result.Error ?? "We could not publish the results.";

        return RedirectToAction(nameof(Results), new { id = eventId });
    }

    // -----------------------------------------------------------------------

    /// <summary>Uploads a banner; returns an error message when the upload fails.</summary>
    private async Task<string?> TryUploadBannerAsync(int eventId, IFormFile? banner)
    {
        if (banner is null || banner.Length == 0) return null;

        if (banner.Length > 5 * 1024 * 1024) return "The banner image must be 5 MB or smaller.";

        if (!AllowedImageTypes.Contains(banner.ContentType.ToLowerInvariant()))
        {
            return "Only JPG, PNG, WEBP or GIF images are supported.";
        }

        await using var stream = banner.OpenReadStream();
        var result = await _api.UploadBannerAsync(eventId, stream, banner.FileName, banner.ContentType);

        return result.Success ? null : result.Error ?? "We could not upload the banner image.";
    }
}
