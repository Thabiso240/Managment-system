using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private static readonly string[] AllowedImageTypes = { "image/jpeg", "image/png", "image/webp", "image/gif" };
    private const long MaxImageBytes = 5 * 1024 * 1024;

    private readonly RaceDayDbContext _db;
    private readonly IFileStorageService _storage;

    public EventsController(RaceDayDbContext db, IFileStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    private IQueryable<Event> BaseQuery() => _db.Events
        .Include(e => e.Organiser)
        .Include(e => e.Categories)
        .Include(e => e.Enrolments);

    /// <summary>Public list of events, with optional search, type and location filters.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EventDto>>> GetEvents(
        [FromQuery] string? search,
        [FromQuery] string? type,
        [FromQuery] string? location,
        [FromQuery] bool? upcoming)
    {
        var query = BaseQuery();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(e => e.Name.Contains(term) || e.Location.Contains(term) || e.Description.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<EventType>(type, true, out var parsedType))
        {
            query = query.Where(e => e.EventType == parsedType);
        }

        if (!string.IsNullOrWhiteSpace(location))
        {
            var loc = location.Trim();
            query = query.Where(e => e.Location.Contains(loc));
        }

        if (upcoming == true)
        {
            query = query.Where(e => e.Date >= DateTime.UtcNow.Date);
        }

        var events = await query.OrderBy(e => e.Date).ToListAsync();
        return Ok(events.Select(e => e.ToDto()));
    }

    /// <summary>Events belonging to the signed-in Organiser.</summary>
    [Authorize(Roles = "Organiser")]
    [HttpGet("mine")]
    public async Task<ActionResult<IEnumerable<EventDto>>> GetMyEvents()
    {
        var organiserId = User.GetUserId();
        var events = await BaseQuery()
            .Where(e => e.OrganiserId == organiserId)
            .OrderByDescending(e => e.Date)
            .ToListAsync();

        return Ok(events.Select(e => e.ToDto()));
    }

    /// <summary>Full detail for a single event, including its categories.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<EventDetailDto>> GetEvent(int id)
    {
        var ev = await BaseQuery().FirstOrDefaultAsync(e => e.Id == id);
        return ev is null ? NotFound(new { message = "Event not found." }) : Ok(ev.ToDetailDto());
    }

    [Authorize(Roles = "Organiser")]
    [HttpPost]
    public async Task<ActionResult<EventDto>> CreateEvent([FromBody] CreateEventRequest request)
    {
        var validation = ValidateEvent(request.Name, request.Location, request.DistanceKm, request.Date);
        if (validation is not null) return BadRequest(new { message = validation });

        var ev = new Event
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Date = request.Date.Date,
            Location = request.Location.Trim(),
            DistanceKm = request.DistanceKm,
            EventType = Enum.TryParse<EventType>(request.EventType, true, out var t) ? t : EventType.Road,
            OrganiserId = User.GetUserId()
        };

        _db.Events.Add(ev);
        await _db.SaveChangesAsync();

        var created = await BaseQuery().FirstAsync(e => e.Id == ev.Id);
        return CreatedAtAction(nameof(GetEvent), new { id = ev.Id }, created.ToDto());
    }

    [Authorize(Roles = "Organiser")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<EventDto>> UpdateEvent(int id, [FromBody] UpdateEventRequest request)
    {
        var ev = await _db.Events.FirstOrDefaultAsync(e => e.Id == id);
        if (ev is null) return NotFound(new { message = "Event not found." });
        if (ev.OrganiserId != User.GetUserId()) return Forbid();

        var validation = ValidateEvent(request.Name, request.Location, request.DistanceKm, request.Date);
        if (validation is not null) return BadRequest(new { message = validation });

        ev.Name = request.Name.Trim();
        ev.Description = request.Description?.Trim() ?? string.Empty;
        ev.Date = request.Date.Date;
        ev.Location = request.Location.Trim();
        ev.DistanceKm = request.DistanceKm;
        ev.EventType = Enum.TryParse<EventType>(request.EventType, true, out var t) ? t : ev.EventType;

        await _db.SaveChangesAsync();

        var updated = await BaseQuery().FirstAsync(e => e.Id == ev.Id);
        return Ok(updated.ToDto());
    }

    [Authorize(Roles = "Organiser")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var ev = await _db.Events.FirstOrDefaultAsync(e => e.Id == id);
        if (ev is null) return NotFound(new { message = "Event not found." });
        if (ev.OrganiserId != User.GetUserId()) return Forbid();

        _db.Events.Remove(ev);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Uploads an event banner image through the API's blob storage service.</summary>
    [Authorize(Roles = "Organiser")]
    [HttpPost("{id:int}/banner")]
    public async Task<ActionResult<EventDto>> UploadBanner(int id, IFormFile file)
    {
        var ev = await _db.Events.FirstOrDefaultAsync(e => e.Id == id);
        if (ev is null) return NotFound(new { message = "Event not found." });
        if (ev.OrganiserId != User.GetUserId()) return Forbid();

        if (file is null || file.Length == 0) return BadRequest(new { message = "Please choose an image file to upload." });
        if (file.Length > MaxImageBytes) return BadRequest(new { message = "The banner image must be 5 MB or smaller." });
        if (!AllowedImageTypes.Contains(file.ContentType.ToLowerInvariant())) return BadRequest(new { message = "Only JPG, PNG, WEBP or GIF images are supported." });

        await using var stream = file.OpenReadStream();
        var url = await _storage.UploadAsync(stream, file.FileName, file.ContentType, "banners");

        ev.BannerImageUrl = url;
        await _db.SaveChangesAsync();

        var updated = await BaseQuery().FirstAsync(e => e.Id == ev.Id);
        return Ok(updated.ToDto());
    }

    private static string? ValidateEvent(string? name, string? location, decimal distance, DateTime date)
    {
        if (string.IsNullOrWhiteSpace(name)) return "Event name is required.";
        if (string.IsNullOrWhiteSpace(location)) return "Event location is required.";
        if (distance <= 0) return "Distance must be greater than zero.";
        if (date == default) return "A valid event date is required.";
        return null;
    }
}
