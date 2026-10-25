using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public EnrolmentsController(RaceDayDbContext db) => _db = db;

    /// <summary>Enrols the signed-in Participant in an event (optionally selecting a category).</summary>
    [Authorize(Roles = "Participant")]
    [HttpPost("api/events/{eventId:int}/enrolments")]
    public async Task<ActionResult<EnrolmentDto>> Enrol(int eventId, [FromBody] CreateEnrolmentRequest request)
    {
        var participantId = User.GetUserId();

        var ev = await _db.Events.Include(e => e.Categories).FirstOrDefaultAsync(e => e.Id == eventId);
        if (ev is null) return NotFound(new { message = "Event not found." });

        if (ev.Date.Date < DateTime.UtcNow.Date)
        {
            return BadRequest(new { message = "This event has already taken place and is closed for entries." });
        }

        if (request.CategoryId is int categoryId)
        {
            var category = ev.Categories.FirstOrDefault(c => c.Id == categoryId);
            if (category is null) return BadRequest(new { message = "The selected category does not belong to this event." });

            if (category.MaxEntries is int max)
            {
                var taken = await _db.Enrolments.CountAsync(en => en.CategoryId == categoryId && en.Status != EnrolmentStatus.Cancelled);
                if (taken >= max) return BadRequest(new { message = "The selected category is full." });
            }
        }

        // Re-use an existing row so that a cancelled entry can be reinstated.
        var existing = await _db.Enrolments.FirstOrDefaultAsync(en => en.EventId == eventId && en.ParticipantId == participantId);
        if (existing is not null && existing.Status != EnrolmentStatus.Cancelled)
        {
            return Conflict(new { message = "You are already enrolled for this event." });
        }

        if (existing is not null)
        {
            existing.Status = EnrolmentStatus.Pending;
            existing.CategoryId = request.CategoryId;
            existing.EnrolledAt = DateTime.UtcNow;
        }
        else
        {
            existing = new Enrolment
            {
                EventId = eventId,
                ParticipantId = participantId,
                CategoryId = request.CategoryId,
                Status = EnrolmentStatus.Pending
            };
            _db.Enrolments.Add(existing);
        }

        await _db.SaveChangesAsync();

        var saved = await _db.Enrolments
            .Include(en => en.Event)
            .Include(en => en.Category)
            .FirstAsync(en => en.Id == existing.Id);

        return Ok(saved.ToDto());
    }

    /// <summary>All enrolments belonging to the signed-in Participant.</summary>
    [Authorize(Roles = "Participant")]
    [HttpGet("api/enrolments/mine")]
    public async Task<ActionResult<IEnumerable<EnrolmentDto>>> MyEnrolments()
    {
        var participantId = User.GetUserId();
        var enrolments = await _db.Enrolments
            .Include(en => en.Event)
            .Include(en => en.Category)
            .Where(en => en.ParticipantId == participantId)
            .OrderByDescending(en => en.EnrolledAt)
            .ToListAsync();

        return Ok(enrolments.Select(en => en.ToDto()));
    }

    /// <summary>Cancels one of the Participant's own enrolments.</summary>
    [Authorize(Roles = "Participant")]
    [HttpDelete("api/enrolments/{id:int}")]
    public async Task<IActionResult> Cancel(int id)
    {
        var enrolment = await _db.Enrolments.FirstOrDefaultAsync(en => en.Id == id);
        if (enrolment is null) return NotFound(new { message = "Enrolment not found." });
        if (enrolment.ParticipantId != User.GetUserId()) return Forbid();

        enrolment.Status = EnrolmentStatus.Cancelled;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>All enrolments for one of the Organiser's events, with category and status.</summary>
    [Authorize(Roles = "Organiser")]
    [HttpGet("api/events/{eventId:int}/enrolments")]
    public async Task<ActionResult<IEnumerable<EventEnrolmentDto>>> ForEvent(int eventId)
    {
        var ev = await _db.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        if (ev is null) return NotFound(new { message = "Event not found." });
        if (ev.OrganiserId != User.GetUserId()) return Forbid();

        var enrolments = await _db.Enrolments
            .Include(en => en.Participant)
            .Include(en => en.Category)
            .Include(en => en.Result)
            .Where(en => en.EventId == eventId)
            .OrderBy(en => en.Participant!.LastName)
            .ToListAsync();

        return Ok(enrolments.Select(en => en.ToOrganiserDto()));
    }
}
