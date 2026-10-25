using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public CategoriesController(RaceDayDbContext db) => _db = db;

    /// <summary>Lists the categories available for an event (public).</summary>
    [HttpGet("api/events/{eventId:int}/categories")]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetForEvent(int eventId)
    {
        var categories = await _db.Categories
            .Include(c => c.Enrolments)
            .Where(c => c.EventId == eventId)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return Ok(categories.Select(c => c.ToDto()));
    }

    [Authorize(Roles = "Organiser")]
    [HttpPost("api/events/{eventId:int}/categories")]
    public async Task<ActionResult<CategoryDto>> Create(int eventId, [FromBody] CreateCategoryRequest request)
    {
        var ev = await _db.Events.FirstOrDefaultAsync(e => e.Id == eventId);
        if (ev is null) return NotFound(new { message = "Event not found." });
        if (ev.OrganiserId != User.GetUserId()) return Forbid();
        if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest(new { message = "Category name is required." });

        var category = new Category
        {
            EventId = eventId,
            Name = request.Name.Trim(),
            AgeGroup = request.AgeGroup,
            DistanceKm = request.DistanceKm,
            EntryFee = request.EntryFee,
            MaxEntries = request.MaxEntries
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        return Ok(category.ToDto());
    }

    [Authorize(Roles = "Organiser")]
    [HttpPut("api/categories/{id:int}")]
    public async Task<ActionResult<CategoryDto>> Update(int id, [FromBody] UpdateCategoryRequest request)
    {
        var category = await _db.Categories.Include(c => c.Event).Include(c => c.Enrolments).FirstOrDefaultAsync(c => c.Id == id);
        if (category is null) return NotFound(new { message = "Category not found." });
        if (category.Event?.OrganiserId != User.GetUserId()) return Forbid();
        if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest(new { message = "Category name is required." });

        category.Name = request.Name.Trim();
        category.AgeGroup = request.AgeGroup;
        category.DistanceKm = request.DistanceKm;
        category.EntryFee = request.EntryFee;
        category.MaxEntries = request.MaxEntries;

        await _db.SaveChangesAsync();
        return Ok(category.ToDto());
    }

    [Authorize(Roles = "Organiser")]
    [HttpDelete("api/categories/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _db.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.Id == id);
        if (category is null) return NotFound(new { message = "Category not found." });
        if (category.Event?.OrganiserId != User.GetUserId()) return Forbid();

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
