using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
using RaceDay.Api.Services;
namespace RaceDay.Api.Controllers;
[ApiController,Route("api/events")]
public class EventsController(RaceDayDbContext db) : ControllerBase
{
    /// <summary>Lists events available for browsing.</summary>
    [HttpGet] public async Task<ActionResult<IEnumerable<EventResponse>>> GetAll()=>Ok(await db.Events.AsNoTracking().OrderBy(x=>x.EventDate).Select(x=>new EventResponse(x.EventId,x.OrganiserId,x.EventName,x.Description,x.EventDate,x.Venue,x.City,x.Province,x.EventType,x.Status,x.CreatedAt)).ToListAsync());
    /// <summary>Returns an event with its categories and routes.</summary>
    [HttpGet("{id:int}")] public async Task<ActionResult<EventDetailsResponse>> Get(int id){var e=await db.Events.AsNoTracking().Include(x=>x.Categories).Include(x=>x.Routes).SingleOrDefaultAsync(x=>x.EventId==id);if(e is null)return NotFound();return Ok(new EventDetailsResponse(Map(e),e.Categories.Select(Map),e.Routes.Select(Map)));}
    /// <summary>Creates an event for the authenticated organiser.</summary>
    [Authorize(Roles="Organiser"),HttpPost] public async Task<ActionResult<EventResponse>> Create(EventRequest request){var validation=Validate(request);if(validation is not null)return BadRequest(new{message=validation});var e=new Event{OrganiserId=User.UserId(),EventName=request.EventName.Trim(),Description=request.Description?.Trim(),EventDate=request.EventDate.Date,Venue=request.Venue.Trim(),City=request.City.Trim(),Province=request.Province.Trim(),EventType=request.EventType,Status=request.Status};db.Events.Add(e);await db.SaveChangesAsync();return CreatedAtAction(nameof(Get),new{id=e.EventId},Map(e));}
    /// <summary>Updates an event owned by the authenticated organiser.</summary>
    [Authorize(Roles="Organiser"),HttpPut("{id:int}")] public async Task<ActionResult<EventResponse>> Update(int id,EventRequest request){var e=await db.Events.FindAsync(id);if(e is null)return NotFound();if(e.OrganiserId!=User.UserId())return Forbid();var validation=Validate(request);if(validation is not null)return BadRequest(new{message=validation});e.EventName=request.EventName.Trim();e.Description=request.Description?.Trim();e.EventDate=request.EventDate.Date;e.Venue=request.Venue.Trim();e.City=request.City.Trim();e.Province=request.Province.Trim();e.EventType=request.EventType;e.Status=request.Status;await db.SaveChangesAsync();return Ok(Map(e));}
    /// <summary>Deletes an event owned by the authenticated organiser.</summary>
    [Authorize(Roles="Organiser"),HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int id){var e=await db.Events.FindAsync(id);if(e is null)return NotFound();if(e.OrganiserId!=User.UserId())return Forbid();db.Events.Remove(e);await db.SaveChangesAsync();return NoContent();}
    /// <summary>Lists routes belonging to an event.</summary>
    [HttpGet("{eventId:int}/routes")] public async Task<ActionResult<IEnumerable<RouteResponse>>> Routes(int eventId){if(!await db.Events.AnyAsync(x=>x.EventId==eventId))return NotFound();return Ok(await db.EventRoutes.AsNoTracking().Where(x=>x.EventId==eventId).Select(x=>new RouteResponse(x.RouteId,x.EventId,x.RouteName,x.StartLocation,x.FinishLocation,x.RouteDistanceKm,x.RouteDescription)).ToListAsync());}
    /// <summary>Adds route information to an organiser's event.</summary>
    [Authorize(Roles="Organiser"),HttpPost("{eventId:int}/routes")] public async Task<ActionResult<RouteResponse>> AddRoute(int eventId,RouteRequest request){var e=await db.Events.FindAsync(eventId);if(e is null)return NotFound();if(e.OrganiserId!=User.UserId())return Forbid();if(request.RouteDistanceKm<=0)return BadRequest(new{message="Route distance must be greater than zero."});if(await db.EventRoutes.AnyAsync(x=>x.EventId==eventId&&x.RouteName==request.RouteName.Trim()))return Conflict(new{message="Route name already exists for this event."});var r=new EventRoute{EventId=eventId,RouteName=request.RouteName.Trim(),StartLocation=request.StartLocation.Trim(),FinishLocation=request.FinishLocation.Trim(),RouteDistanceKm=request.RouteDistanceKm,RouteDescription=request.RouteDescription?.Trim()};db.EventRoutes.Add(r);await db.SaveChangesAsync();return Created($"/api/routes/{r.RouteId}",Map(r));}
    static string? Validate(EventRequest r)=>r.EventDate.Date<DateTime.UtcNow.Date?"Event date cannot be in the past.":r.EventType is not("Running" or "Walking" or "Cycling")?"Event type must be Running, Walking or Cycling.":r.Status is not("Draft" or "Upcoming" or "Completed" or "Cancelled")?"Invalid event status.":null;
    static EventResponse Map(Event e)=>new(e.EventId,e.OrganiserId,e.EventName,e.Description,e.EventDate,e.Venue,e.City,e.Province,e.EventType,e.Status,e.CreatedAt);
    static RouteResponse Map(EventRoute r)=>new(r.RouteId,r.EventId,r.RouteName,r.StartLocation,r.FinishLocation,r.RouteDistanceKm,r.RouteDescription);
    static CategoryResponse Map(Category c)=>new(c.CategoryId,c.EventId,c.CategoryName,c.DistanceKm,c.EntryFee,c.MaximumParticipants);
}
