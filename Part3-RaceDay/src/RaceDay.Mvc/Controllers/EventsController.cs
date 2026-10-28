using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Filters;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

/// <summary>Full event information and the participant enrolment action.</summary>
public class EventsController : Controller
{
    private readonly ApiClient _api;

    public EventsController(ApiClient api) => _api = api;

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var ev = await _api.GetEventAsync(id);
        if (ev is null) return NotFound();

        if (SessionUser.IsParticipant(HttpContext.Session))
        {
            var mine = await _api.GetMyEnrolmentsAsync();
            ViewBag.MyEnrolment = mine.FirstOrDefault(e => e.EventId == id && e.Status != "Cancelled");
        }

        return View(ev);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequireLogin(Role = "Participant")]
    public async Task<IActionResult> Enrol(int eventId, int? categoryId)
    {
        var result = await _api.EnrolAsync(eventId, categoryId);

        if (result.Success)
        {
            TempData["Success"] = "Your entry has been recorded. You can see it under My Events.";
        }
        else
        {
            TempData["Error"] = result.Error ?? "We could not record your entry.";
        }

        return RedirectToAction(nameof(Details), new { id = eventId });
    }
}
