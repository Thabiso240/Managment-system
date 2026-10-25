using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Authorize(Roles = "Organiser")]
[Route("api/organiser")]
public class OrganiserController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public OrganiserController(RaceDayDbContext db) => _db = db;

    /// <summary>
    /// Summary of everything the Organiser owns: total events, total enrolments and
    /// a per-event breakdown used by the dashboard.
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<OrganiserDashboardDto>> Dashboard()
    {
        var organiserId = User.GetUserId();

        var events = await _db.Events
            .Include(e => e.Categories)
            .Include(e => e.Enrolments)
            .Where(e => e.OrganiserId == organiserId)
            .OrderBy(e => e.Date)
            .ToListAsync();

        var today = DateTime.UtcNow.Date;

        var summaries = events.Select(e => new EventSummaryDto(
            e.Id,
            e.Name,
            e.Date,
            e.Location,
            e.EventType.ToString(),
            e.Enrolments.Count,
            e.Enrolments.Count(en => en.Status == EnrolmentStatus.Confirmed),
            e.Enrolments.Count(en => en.Status == EnrolmentStatus.Pending),
            e.Categories.Count,
            e.Date < today)).ToList();

        return Ok(new OrganiserDashboardDto(
            events.Count,
            summaries.Sum(s => s.EnrolmentCount),
            summaries.Count(s => !s.IsPast),
            summaries));
    }
}
