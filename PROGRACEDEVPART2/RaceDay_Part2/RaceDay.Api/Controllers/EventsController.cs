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
    private readonly RaceDayDbContext _db;

    public EventsController(RaceDayDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EventResponse>>> GetAll()
    {
        var events = await _db.Events
            .AsNoTracking()
            .OrderBy(x => x.EventDate)
            .Select(x => new EventResponse(
                x.EventId,
                x.OrganiserId,
                x.EventName,
                x.Description,
                x.EventDate,
                x.Venue,
                x.City,
                x.Province,
                x.EventType,
                x.Status,
                x.CreatedAt))
            .ToListAsync();

        return Ok(events);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EventDetailsResponse>> GetById(int id)
    {
        var eventEntity = await _db.Events
            .AsNoTracking()
            .Include(x => x.Categories)
            .Include(x => x.Routes)
            .SingleOrDefaultAsync(x => x.EventId == id);

        if (eventEntity is null)
            return NotFound();

        return Ok(new EventDetailsResponse(
            new EventResponse(
                eventEntity.EventId,
                eventEntity.OrganiserId,
                eventEntity.EventName,
                eventEntity.Description,
                eventEntity.EventDate,
                eventEntity.Venue,
                eventEntity.City,
                eventEntity.Province,
                eventEntity.EventType,
                eventEntity.Status,
                eventEntity.CreatedAt),
            eventEntity.Categories.Select(c => new CategoryResponse(
                c.CategoryId,
                c.EventId,
                c.CategoryName,
                c.DistanceKm,
                c.EntryFee,
                c.MaximumParticipants)).ToList(),
            eventEntity.Routes.Select(r => new RouteResponse(
                r.RouteId,
                r.EventId,
                r.RouteName,
                r.StartLocation,
                r.FinishLocation,
                r.RouteDistanceKm,
                r.RouteDescription)).ToList()));
    }

    [Authorize(Roles = "Organiser")]
    [HttpPost]
    public async Task<ActionResult<EventResponse>> Create(EventRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var validationError = Validate(request);

        if (validationError is not null)
            return BadRequest(new { message = validationError });

        var organiserId = User.UserId();

        var newEvent = new Event
        {
            OrganiserId = organiserId,
            EventName = request.EventName.Trim(),
            Description = request.Description,
            EventDate = request.EventDate,
            Venue = request.Venue.Trim(),
            City = request.City.Trim(),
            Province = request.Province.Trim(),
            EventType = request.EventType.Trim(),
            Status = request.Status.Trim()
        };

        _db.Events.Add(newEvent);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = newEvent.EventId }, new EventResponse(
            newEvent.EventId,
            newEvent.OrganiserId,
            newEvent.EventName,
            newEvent.Description,
            newEvent.EventDate,
            newEvent.Venue,
            newEvent.City,
            newEvent.Province,
            newEvent.EventType,
            newEvent.Status,
            newEvent.CreatedAt));
    }

    [Authorize(Roles = "Organiser")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<EventResponse>> Update(int id, EventRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var validationError = Validate(request);

        if (validationError is not null)
            return BadRequest(new { message = validationError });

        var eventEntity = await _db.Events.FindAsync(id);

        if (eventEntity is null)
            return NotFound();

        if (eventEntity.OrganiserId != User.UserId())
            return Forbid();

        eventEntity.EventName = request.EventName.Trim();
        eventEntity.Description = request.Description;
        eventEntity.EventDate = request.EventDate;
        eventEntity.Venue = request.Venue.Trim();
        eventEntity.City = request.City.Trim();
        eventEntity.Province = request.Province.Trim();
        eventEntity.EventType = request.EventType.Trim();
        eventEntity.Status = request.Status.Trim();

        await _db.SaveChangesAsync();

        return Ok(new EventResponse(
            eventEntity.EventId,
            eventEntity.OrganiserId,
            eventEntity.EventName,
            eventEntity.Description,
            eventEntity.EventDate,
            eventEntity.Venue,
            eventEntity.City,
            eventEntity.Province,
            eventEntity.EventType,
            eventEntity.Status,
            eventEntity.CreatedAt));
    }

    [Authorize(Roles = "Organiser")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var eventEntity = await _db.Events.FindAsync(id);

        if (eventEntity is null)
            return NotFound();

        if (eventEntity.OrganiserId != User.UserId())
            return Forbid();

        _db.Events.Remove(eventEntity);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{eventId:int}/routes")]
    public async Task<ActionResult<IEnumerable<RouteResponse>>> GetRoutes(int eventId)
    {
        var eventEntity = await _db.Events.AnyAsync(x => x.EventId == eventId);

        if (!eventEntity)
            return NotFound();

        return Ok(await _db.EventRoutes
            .AsNoTracking()
            .Where(x => x.EventId == eventId)
            .OrderBy(x => x.RouteName)
            .Select(x => new RouteResponse(
                x.RouteId,
                x.EventId,
                x.RouteName,
                x.StartLocation,
                x.FinishLocation,
                x.RouteDistanceKm,
                x.RouteDescription))
            .ToListAsync());
    }

    [Authorize(Roles = "Organiser")]
    [HttpPost("{eventId:int}/routes")]
    public async Task<ActionResult<RouteResponse>> AddRoute(int eventId, RouteRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var eventEntity = await _db.Events.SingleOrDefaultAsync(x => x.EventId == eventId);

        if (eventEntity is null)
            return NotFound();

        if (eventEntity.OrganiserId != User.UserId())
            return Forbid();

        if (string.IsNullOrWhiteSpace(request.RouteName))
            return BadRequest(new { message = "Route name is required." });

        if (request.RouteDistanceKm <= 0)
            return BadRequest(new { message = "Route distance must be greater than zero." });

        var route = new EventRoute
        {
            EventId = eventId,
            RouteName = request.RouteName.Trim(),
            StartLocation = request.StartLocation.Trim(),
            FinishLocation = request.FinishLocation.Trim(),
            RouteDistanceKm = request.RouteDistanceKm,
            RouteDescription = request.RouteDescription
        };

        _db.EventRoutes.Add(route);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRoutes), new { eventId }, route);
    }

    private static string? Validate(EventRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.EventName))
            return "Event name is required.";

        if (string.IsNullOrWhiteSpace(request.Venue))
            return "Venue is required.";

        if (string.IsNullOrWhiteSpace(request.City))
            return "City is required.";

        if (string.IsNullOrWhiteSpace(request.Province))
            return "Province is required.";

        if (request.EventDate.Date < DateTime.UtcNow.Date)
            return "Event date cannot be in the past.";

        if (request.EventType is not ("Running" or "Walking" or "Cycling"))
            return "Event type must be Running, Walking or Cycling.";

        if (request.Status is not ("Upcoming" or "Draft" or "Completed" or "Cancelled"))
            return "Status must be Upcoming, Draft, Completed or Cancelled.";

        return null;
    }
}
