using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public CategoriesController(RaceDayDbContext db)
    {
        _db = db;
    }

    [HttpGet("api/events/{eventId:int}/categories")]
    public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetForEvent(int eventId)
    {
        var exists = await _db.Events.AnyAsync(x => x.EventId == eventId);

        if (!exists)
            return NotFound();

        var categories = await _db.Categories
            .AsNoTracking()
            .Where(x => x.EventId == eventId)
            .OrderBy(x => x.CategoryName)
            .Select(x => new CategoryResponse(
                x.CategoryId,
                x.EventId,
                x.CategoryName,
                x.DistanceKm,
                x.EntryFee,
                x.MaximumParticipants))
            .ToListAsync();

        return Ok(categories);
    }

    [Authorize(Roles = "Organiser")]
    [HttpPost("api/events/{eventId:int}/categories")]
    public async Task<ActionResult<CategoryResponse>> Create(int eventId, CategoryRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var eventEntity = await _db.Events.SingleOrDefaultAsync(x => x.EventId == eventId);

        if (eventEntity is null)
            return NotFound();

        if (eventEntity.OrganiserId != User.UserId())
            return Forbid();

        if (string.IsNullOrWhiteSpace(request.CategoryName))
            return BadRequest(new { message = "Category name is required." });

        if (request.DistanceKm <= 0)
            return BadRequest(new { message = "Distance must be greater than zero." });

        if (request.EntryFee < 0)
            return BadRequest(new { message = "Entry fee cannot be negative." });

        if (request.MaximumParticipants.HasValue && request.MaximumParticipants <= 0)
            return BadRequest(new { message = "Maximum participants must be greater than zero." });

        var category = new Category
        {
            EventId = eventId,
            CategoryName = request.CategoryName.Trim(),
            DistanceKm = request.DistanceKm,
            EntryFee = request.EntryFee,
            MaximumParticipants = request.MaximumParticipants
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetForEvent), new { eventId }, new CategoryResponse(
            category.CategoryId,
            category.EventId,
            category.CategoryName,
            category.DistanceKm,
            category.EntryFee,
            category.MaximumParticipants));
    }

    [Authorize(Roles = "Organiser")]
    [HttpPut("api/categories/{id:int}")]
    public async Task<ActionResult<CategoryResponse>> Update(int id, CategoryRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var category = await _db.Categories
            .Include(x => x.Event)
            .SingleOrDefaultAsync(x => x.CategoryId == id);

        if (category is null)
            return NotFound();

        if (category.Event.OrganiserId != User.UserId())
            return Forbid();

        if (string.IsNullOrWhiteSpace(request.CategoryName))
            return BadRequest(new { message = "Category name is required." });

        if (request.DistanceKm <= 0)
            return BadRequest(new { message = "Distance must be greater than zero." });

        if (request.EntryFee < 0)
            return BadRequest(new { message = "Entry fee cannot be negative." });

        if (request.MaximumParticipants.HasValue && request.MaximumParticipants <= 0)
            return BadRequest(new { message = "Maximum participants must be greater than zero." });

        category.CategoryName = request.CategoryName.Trim();
        category.DistanceKm = request.DistanceKm;
        category.EntryFee = request.EntryFee;
        category.MaximumParticipants = request.MaximumParticipants;

        await _db.SaveChangesAsync();

        return Ok(new CategoryResponse(
            category.CategoryId,
            category.EventId,
            category.CategoryName,
            category.DistanceKm,
            category.EntryFee,
            category.MaximumParticipants));
    }

    [Authorize(Roles = "Organiser")]
    [HttpDelete("api/categories/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _db.Categories
            .Include(x => x.Event)
            .SingleOrDefaultAsync(x => x.CategoryId == id);

        if (category is null)
            return NotFound();

        if (category.Event.OrganiserId != User.UserId())
            return Forbid();

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
