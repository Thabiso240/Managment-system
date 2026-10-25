using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
public class ResultsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public ResultsController(RaceDayDbContext db) => _db = db;

    /// <summary>Captures (or updates) a Participant's finish time and position.</summary>
    [Authorize(Roles = "Organiser")]
    [HttpPost("api/events/{eventId:int}/results")]
    public async Task<ActionResult<EventEnrolmentDto>> Record(int eventId, [FromBody] RecordResultRequest request)
    {
        var ev = await _db.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        if (ev is null) return NotFound(new { message = "Event not found." });
        if (ev.OrganiserId != User.GetUserId()) return Forbid();

        var enrolment = await _db.Enrolments
            .Include(en => en.Participant)
            .Include(en => en.Category)
            .Include(en => en.Result)
            .FirstOrDefaultAsync(en => en.Id == request.EnrolmentId && en.EventId == eventId);

        if (enrolment is null) return NotFound(new { message = "Enrolment not found for this event." });

        if (!TryParseTime(request.FinishTime, out var finishTime))
        {
            return BadRequest(new { message = "Finish time must be in hh:mm:ss or mm:ss format." });
        }

        if (request.Position < 1)
        {
            return BadRequest(new { message = "Finishing position must be 1 or greater." });
        }

        if (enrolment.Result is null)
        {
            enrolment.Result = new RaceResult
            {
                EnrolmentId = enrolment.Id,
                FinishTime = finishTime,
                Position = request.Position
            };
        }
        else
        {
            enrolment.Result.FinishTime = finishTime;
            enrolment.Result.Position = request.Position;
            enrolment.Result.RecordedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return Ok(enrolment.ToOrganiserDto());
    }

    /// <summary>Publishes all captured results for an event, making them visible to Participants.</summary>
    [Authorize(Roles = "Organiser")]
    [HttpPost("api/events/{eventId:int}/results/publish")]
    public async Task<IActionResult> Publish(int eventId, [FromBody] PublishResultsRequest request)
    {
        var ev = await _db.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        if (ev is null) return NotFound(new { message = "Event not found." });
        if (ev.OrganiserId != User.GetUserId()) return Forbid();

        var results = await _db.Results
            .Include(r => r.Enrolment)
            .Where(r => r.Enrolment!.EventId == eventId)
            .ToListAsync();

        if (results.Count == 0)
        {
            return BadRequest(new { message = "There are no captured results to publish yet." });
        }

        foreach (var result in results)
        {
            result.IsPublished = true;
            result.TotalFinishers = request.TotalFinishers > 0 ? request.TotalFinishers : results.Count;
        }

        await _db.SaveChangesAsync();
        return Ok(new { message = $"Published {results.Count} results.", count = results.Count });
    }

    /// <summary>The signed-in Participant's personal race history (published results only).</summary>
    [Authorize(Roles = "Participant")]
    [HttpGet("api/participants/me/results")]
    public async Task<ActionResult<IEnumerable<ParticipantResultDto>>> MyResults()
    {
        var participantId = User.GetUserId();

        var results = await _db.Results
            .Include(r => r.Enrolment)!.ThenInclude(en => en!.Event)
            .Include(r => r.Enrolment)!.ThenInclude(en => en!.Category)
            .Where(r => r.IsPublished && r.Enrolment!.ParticipantId == participantId)
            .OrderByDescending(r => r.Enrolment!.Event!.Date)
            .ToListAsync();

        var dto = results.Select(r => new ParticipantResultDto(
            r.Enrolment!.EventId,
            r.Enrolment.Event!.Name,
            r.Enrolment.Event.Date,
            r.Enrolment.Event.Location,
            r.Enrolment.Event.EventType.ToString(),
            r.Enrolment.Category?.Name,
            r.FinishTime.ToString(@"hh\:mm\:ss"),
            r.Position,
            r.TotalFinishers));

        return Ok(dto);
    }

    /// <summary>Accepts "h:mm:ss" or "mm:ss" and converts it to a <see cref="TimeSpan"/>.</summary>
    private static bool TryParseTime(string? text, out TimeSpan time)
    {
        time = default;
        if (string.IsNullOrWhiteSpace(text)) return false;

        var parts = text.Trim().Split(':');

        if (parts.Length == 3
            && int.TryParse(parts[0], out var hours)
            && int.TryParse(parts[1], out var minutes)
            && int.TryParse(parts[2], out var seconds)
            && minutes is >= 0 and < 60 && seconds is >= 0 and < 60)
        {
            time = new TimeSpan(hours, minutes, seconds);
            return true;
        }

        if (parts.Length == 2
            && int.TryParse(parts[0], out var mins)
            && int.TryParse(parts[1], out var secs)
            && secs is >= 0 and < 60)
        {
            time = new TimeSpan(0, mins, secs);
            return true;
        }

        return false;
    }
}
