using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[ApiController]
[Authorize(Roles = "Participant")]
[Route("api/enrolments")]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public EnrolmentsController(RaceDayDbContext db)
    {
        _db = db;
    }

    [HttpGet("me")]
    public async Task<ActionResult<IEnumerable<EnrolmentResponse>>> GetMyEnrolments()
    {
        var userId = User.UserId();

        var enrolments = await _db.Enrolments
            .AsNoTracking()
            .Include(x => x.Category)
            .ThenInclude(x => x.Event)
            .Where(x => x.ParticipantId == userId)
            .OrderByDescending(x => x.EnrolmentDate)
            .Select(x => new EnrolmentResponse(
                x.EnrolmentId,
                x.ParticipantId,
                x.CategoryId,
                x.Category.CategoryName,
                x.Category.EventId,
                x.Category.Event.EventName,
                x.EnrolmentDate,
                x.EmergencyContactName,
                x.EmergencyContactPhone,
                x.PaymentStatus,
                x.EnrolmentStatus))
            .ToListAsync();

        return Ok(enrolments);
    }

    [HttpPost]
    public async Task<ActionResult<EnrolmentResponse>> Create(EnrolmentRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var participantId = User.UserId();

        var category = await _db.Categories
            .Include(x => x.Event)
            .SingleOrDefaultAsync(x => x.CategoryId == request.CategoryId);

        if (category is null)
            return NotFound(new { message = "Category not found." });

        var alreadyExists = await _db.Enrolments.AnyAsync(x =>
            x.ParticipantId == participantId &&
            x.CategoryId == category.CategoryId);

        if (alreadyExists)
            return Conflict(new { message = "Duplicate enrolment for this category." });

        var enrolment = new Enrolment
        {
            ParticipantId = participantId,
            CategoryId = category.CategoryId,
            EnrolmentDate = DateTime.UtcNow,
            EmergencyContactName = request.EmergencyContactName,
            EmergencyContactPhone = request.EmergencyContactPhone,
            PaymentStatus = "Pending",
            EnrolmentStatus = "Active"
        };

        _db.Enrolments.Add(enrolment);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMyEnrolments), new { }, enrolment);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Cancel(int id)
    {
        var enrolment = await _db.Enrolments
            .SingleOrDefaultAsync(x => x.EnrolmentId == id && x.ParticipantId == User.UserId());

        if (enrolment is null)
            return NotFound();

        _db.Enrolments.Remove(enrolment);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
