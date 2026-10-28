using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

/// <summary>
/// Public home page. Shows all events with filtering so Participants can find a race.
/// </summary>
public class HomeController : Controller
{
    private readonly ApiClient _api;

    public HomeController(ApiClient api) => _api = api;

    public async Task<IActionResult> Index(string? search, string? type, string? location, bool upcomingOnly = false)
    {
        var events = await _api.GetEventsAsync(search, type, location, upcomingOnly);

        ViewBag.Filter = new EventFilterViewModel
        {
            Search = search,
            Type = type,
            Location = location,
            UpcomingOnly = upcomingOnly
        };

        return View(events);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
