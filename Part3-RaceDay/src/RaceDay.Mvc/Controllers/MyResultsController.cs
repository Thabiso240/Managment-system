using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Filters;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

/// <summary>A participant's personal race history across all completed events.</summary>
[RequireLogin(Role = "Participant")]
public class MyResultsController : Controller
{
    private readonly ApiClient _api;

    public MyResultsController(ApiClient api) => _api = api;

    public async Task<IActionResult> Index()
    {
        var results = await _api.GetMyResultsAsync();
        return View(results);
    }
}
