using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
using RaceDay.Api.Services;
namespace RaceDay.Api.Controllers;
[ApiController,Authorize(Roles="Participant"),Route("api/enrolments")]
public class EnrolmentsController(RaceDayDbContext db):ControllerBase
{
    /// <summary>Returns enrolments belonging to the authenticated participant.</summary>
    [HttpGet("me")] public async Task<ActionResult<IEnumerable<EnrolmentResponse>>> Mine()=>Ok(await Query().Where(x=>x.ParticipantId==User.UserId()).OrderByDescending(x=>x.EnrolmentDate).Select(x=>new EnrolmentResponse(x.EnrolmentId,x.ParticipantId,x.CategoryId,x.Category!.CategoryName,x.Category.EventId,x.Category.Event!.EventName,x.EnrolmentDate,x.EmergencyContactName,x.EmergencyContactPhone,x.PaymentStatus,x.EnrolmentStatus)).ToListAsync());
    /// <summary>Enrols the authenticated participant in an event category.</summary>
    [HttpPost] public async Task<ActionResult<EnrolmentResponse>> Create(EnrolmentRequest request){var c=await db.Categories.Include(x=>x.Event).SingleOrDefaultAsync(x=>x.CategoryId==request.CategoryId);if(c is null)return NotFound(new{message="Category not found."});if(c.Event!.EventDate.Date<DateTime.UtcNow.Date)return BadRequest(new{message="The event has already taken place."});if(await db.Enrolments.AnyAsync(x=>x.ParticipantId==User.UserId()&&x.CategoryId==request.CategoryId))return Conflict(new{message="You are already enrolled in this category."});if(c.MaximumParticipants.HasValue&&await db.Enrolments.CountAsync(x=>x.CategoryId==request.CategoryId&&x.EnrolmentStatus=="Active")>=c.MaximumParticipants.Value)return Conflict(new{message="The category is full."});var e=new Enrolment{ParticipantId=User.UserId(),CategoryId=c.CategoryId,EmergencyContactName=request.EmergencyContactName?.Trim(),EmergencyContactPhone=request.EmergencyContactPhone?.Trim()};db.Enrolments.Add(e);await db.SaveChangesAsync();return Created($"/api/enrolments/{e.EnrolmentId}",await Query().Where(x=>x.EnrolmentId==e.EnrolmentId).Select(x=>new EnrolmentResponse(x.EnrolmentId,x.ParticipantId,x.CategoryId,x.Category!.CategoryName,x.Category.EventId,x.Category.Event!.EventName,x.EnrolmentDate,x.EmergencyContactName,x.EmergencyContactPhone,x.PaymentStatus,x.EnrolmentStatus)).SingleAsync());}
    /// <summary>Cancels an enrolment belonging to the authenticated participant.</summary>
    [HttpDelete("{id:int}")] public async Task<IActionResult> Cancel(int id){var e=await db.Enrolments.SingleOrDefaultAsync(x=>x.EnrolmentId==id&&x.ParticipantId==User.UserId());if(e is null)return NotFound();e.EnrolmentStatus="Cancelled";await db.SaveChangesAsync();return NoContent();}
    IQueryable<Enrolment> Query()=>db.Enrolments.AsNoTracking().Include(x=>x.Category).ThenInclude(x=>x.Event);
    static EnrolmentResponse Map(Enrolment x)=>new(x.EnrolmentId,x.ParticipantId,x.CategoryId,x.Category!.CategoryName,x.Category.EventId,x.Category.Event!.EventName,x.EnrolmentDate,x.EmergencyContactName,x.EmergencyContactPhone,x.PaymentStatus,x.EnrolmentStatus);
}
