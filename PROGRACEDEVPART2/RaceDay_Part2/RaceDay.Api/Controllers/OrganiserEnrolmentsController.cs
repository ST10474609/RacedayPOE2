using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Services;
namespace RaceDay.Api.Controllers;
[ApiController,Authorize(Roles="Organiser"),Route("api/events/{eventId:int}/enrolments")]
public class OrganiserEnrolmentsController(RaceDayDbContext db):ControllerBase
{
    /// <summary>Lists all enrolments for an event owned by the organiser.</summary>
    [HttpGet] public async Task<ActionResult<IEnumerable<EnrolmentResponse>>> Get(int eventId){var e=await db.Events.FindAsync(eventId);if(e is null)return NotFound();if(e.OrganiserId!=User.UserId())return Forbid();return Ok(await db.Enrolments.AsNoTracking().Include(x=>x.Participant).Include(x=>x.Category).Where(x=>x.Category!.EventId==eventId).OrderBy(x=>x.EnrolmentDate).Select(x=>new EnrolmentResponse(x.EnrolmentId,x.ParticipantId,x.CategoryId,x.Category!.CategoryName,x.Category.EventId,x.Category.Event!.EventName,x.EnrolmentDate,x.EmergencyContactName,x.EmergencyContactPhone,x.PaymentStatus,x.EnrolmentStatus)).ToListAsync());}
}
