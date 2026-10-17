using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
using RaceDay.Api.Services;
namespace RaceDay.Api.Controllers;
[ApiController]
public class CategoriesController(RaceDayDbContext db):ControllerBase
{
    /// <summary>Lists categories available for an event.</summary>
    [HttpGet("api/events/{eventId:int}/categories")] public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetForEvent(int eventId){if(!await db.Events.AnyAsync(x=>x.EventId==eventId))return NotFound();return Ok(await db.Categories.AsNoTracking().Where(x=>x.EventId==eventId).Select(x=>new CategoryResponse(x.CategoryId,x.EventId,x.CategoryName,x.DistanceKm,x.EntryFee,x.MaximumParticipants)).ToListAsync());}
    /// <summary>Adds a category to an organiser's event.</summary>
    [Authorize(Roles="Organiser"),HttpPost("api/events/{eventId:int}/categories")] public async Task<ActionResult<CategoryResponse>> Create(int eventId,CategoryRequest request){var e=await db.Events.FindAsync(eventId);if(e is null)return NotFound();if(e.OrganiserId!=User.UserId())return Forbid();if(request.DistanceKm<=0||request.EntryFee<0||(request.MaximumParticipants is <=0))return BadRequest(new{message="Category values are invalid."});if(await db.Categories.AnyAsync(x=>x.EventId==eventId&&x.CategoryName==request.CategoryName.Trim()))return Conflict(new{message="Category name already exists for this event."});var c=new Category{EventId=eventId,CategoryName=request.CategoryName.Trim(),DistanceKm=request.DistanceKm,EntryFee=request.EntryFee,MaximumParticipants=request.MaximumParticipants};db.Categories.Add(c);await db.SaveChangesAsync();return Created($"/api/categories/{c.CategoryId}",new CategoryResponse(c.CategoryId,c.EventId,c.CategoryName,c.DistanceKm,c.EntryFee,c.MaximumParticipants));}
    /// <summary>Updates a category owned through an organiser's event.</summary>
    [Authorize(Roles="Organiser"),HttpPut("api/categories/{id:int}")] public async Task<ActionResult<CategoryResponse>> Update(int id,CategoryRequest request){var c=await db.Categories.Include(x=>x.Event).SingleOrDefaultAsync(x=>x.CategoryId==id);if(c is null)return NotFound();if(c.Event!.OrganiserId!=User.UserId())return Forbid();if(request.DistanceKm<=0||request.EntryFee<0||(request.MaximumParticipants is <=0))return BadRequest(new{message="Category values are invalid."});c.CategoryName=request.CategoryName.Trim();c.DistanceKm=request.DistanceKm;c.EntryFee=request.EntryFee;c.MaximumParticipants=request.MaximumParticipants;await db.SaveChangesAsync();return Ok(new CategoryResponse(c.CategoryId,c.EventId,c.CategoryName,c.DistanceKm,c.EntryFee,c.MaximumParticipants));}
    /// <summary>Deletes a category owned through an organiser's event.</summary>
    [Authorize(Roles="Organiser"),HttpDelete("api/categories/{id:int}")] public async Task<IActionResult> Delete(int id){var c=await db.Categories.Include(x=>x.Event).SingleOrDefaultAsync(x=>x.CategoryId==id);if(c is null)return NotFound();if(c.Event!.OrganiserId!=User.UserId())return Forbid();if(await db.Enrolments.AnyAsync(x=>x.CategoryId==id))return Conflict(new{message="A category with enrolments cannot be deleted."});db.Categories.Remove(c);await db.SaveChangesAsync();return NoContent();}
}
