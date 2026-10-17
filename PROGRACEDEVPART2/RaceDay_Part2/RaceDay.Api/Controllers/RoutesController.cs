using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Services;
namespace RaceDay.Api.Controllers;
[ApiController,Route("api/routes")]
public class RoutesController(RaceDayDbContext db):ControllerBase
{
    /// <summary>Updates a route owned through an organiser's event.</summary>
    [Authorize(Roles="Organiser"),HttpPut("{id:int}")] public async Task<ActionResult<RouteResponse>> Update(int id,RouteRequest request){var r=await db.EventRoutes.Include(x=>x.Event).SingleOrDefaultAsync(x=>x.RouteId==id);if(r is null)return NotFound();if(r.Event!.OrganiserId!=User.UserId())return Forbid();if(request.RouteDistanceKm<=0)return BadRequest(new{message="Route distance must be greater than zero."});r.RouteName=request.RouteName.Trim();r.StartLocation=request.StartLocation.Trim();r.FinishLocation=request.FinishLocation.Trim();r.RouteDistanceKm=request.RouteDistanceKm;r.RouteDescription=request.RouteDescription?.Trim();await db.SaveChangesAsync();return Ok(new RouteResponse(r.RouteId,r.EventId,r.RouteName,r.StartLocation,r.FinishLocation,r.RouteDistanceKm,r.RouteDescription));}
    /// <summary>Deletes a route owned through an organiser's event.</summary>
    [Authorize(Roles="Organiser"),HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int id){var r=await db.EventRoutes.Include(x=>x.Event).SingleOrDefaultAsync(x=>x.RouteId==id);if(r is null)return NotFound();if(r.Event!.OrganiserId!=User.UserId())return Forbid();db.EventRoutes.Remove(r);await db.SaveChangesAsync();return NoContent();}
}
