using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Filters;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

/// <summary>A participant's own enrolments and their status.</summary>
[RequireLogin(Role = "Participant")]
public class MyEventsController : Controller
{
    private readonly ApiClient _api;

    public MyEventsController(ApiClient api) => _api = api;

    public async Task<IActionResult> Index()
    {
        var enrolments = await _api.GetMyEnrolmentsAsync();
        return View(enrolments);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await _api.CancelEnrolmentAsync(id);

        if (result.Success)
        {
            TempData["Success"] = "Your entry has been cancelled.";
        }
        else
        {
            TempData["Error"] = result.Error ?? "We could not cancel that entry.";
        }

        return RedirectToAction(nameof(Index));
    }
}
