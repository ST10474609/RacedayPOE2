using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[ApiController]
public class ResultsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public ResultsController(RaceDayDbContext db)
    {
        _db = db;
    }

    [Authorize(Roles = "Participant")]
    [HttpGet("api/results/me")]
    public async Task<ActionResult<IEnumerable<ResultResponse>>> GetMyResults()
    {
        var userId = User.UserId();

        var results = await _db.Results
            .AsNoTracking()
            .Include(x => x.Enrolment)
            .ThenInclude(x => x.Category)
            .ThenInclude(x => x.Event)
            .Where(x => x.Enrolment.ParticipantId == userId)
            .OrderByDescending(x => x.RecordedAt)
            .Select(x => new ResultResponse(
                x.ResultId,
                x.EnrolmentId,
                x.ParticipantId,
                $"{x.Enrolment.Participant.FirstName} {x.Enrolment.Participant.LastName}",
                x.Enrolment.Category.EventId,
                x.Enrolment.Category.Event.EventName,
                x.FinishPosition,
                x.FinishTimeSeconds,
                x.ResultStatus,
                x.RecordedAt))
            .ToListAsync();

        return Ok(results);
    }

    [Authorize(Roles = "Organiser")]
    [HttpGet("api/events/{eventId:int}/results")]
    public async Task<ActionResult<IEnumerable<ResultResponse>>> GetEventResults(int eventId)
    {
        var eventEntity = await _db.Events
            .SingleOrDefaultAsync(x => x.EventId == eventId);

        if (eventEntity is null)
            return NotFound();

        if (eventEntity.OrganiserId != User.UserId())
            return Forbid();

        var results = await _db.Results
            .AsNoTracking()
            .Include(x => x.Enrolment)
            .ThenInclude(x => x.Category)
            .ThenInclude(x => x.Event)
            .Where(x => x.Enrolment.Category.EventId == eventId)
            .OrderByDescending(x => x.RecordedAt)
            .Select(x => new ResultResponse(
                x.ResultId,
                x.EnrolmentId,
                x.Enrolment.ParticipantId,
                $"{x.Enrolment.Participant.FirstName} {x.Enrolment.Participant.LastName}",
                x.Enrolment.Category.EventId,
                x.Enrolment.Category.Event.EventName,
                x.FinishPosition,
                x.FinishTimeSeconds,
                x.ResultStatus,
                x.RecordedAt))
            .ToListAsync();

        return Ok(results);
    }

    [Authorize(Roles = "Organiser")]
    [HttpPost("api/events/{eventId:int}/results")]
    public async Task<ActionResult<ResultResponse>> Create(int eventId, ResultRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var eventEntity = await _db.Events
            .SingleOrDefaultAsync(x => x.EventId == eventId);

        if (eventEntity is null)
            return NotFound();

        if (eventEntity.OrganiserId != User.UserId())
            return Forbid();

        if (request.ResultStatus is not ("Finished" or "DNF" or "DNS" or "Disqualified"))
            return BadRequest(new { message = "Result status must be Finished, DNF, DNS or Disqualified." });

        if (request.FinishPosition.HasValue && request.FinishPosition.Value <= 0)
            return BadRequest(new { message = "Finish position must be greater than zero." });

        if (request.FinishTimeSeconds.HasValue && request.FinishTimeSeconds.Value <= 0)
            return BadRequest(new { message = "Finish time must be greater than zero." });

        var enrolment = await _db.Enrolments
            .Include(x => x.Category)
            .SingleOrDefaultAsync(x => x.EnrolmentId == request.EnrolmentId && x.Category.EventId == eventId);

        if (enrolment is null)
            return BadRequest(new { message = "The enrolment must belong to this event." });

        var existing = await _db.Results.AnyAsync(x => x.EnrolmentId == enrolment.EnrolmentId);

        if (existing)
            return Conflict(new { message = "A result already exists for this enrolment." });

        var result = new Result
        {
            EnrolmentId = enrolment.EnrolmentId,
            FinishPosition = request.FinishPosition,
            FinishTimeSeconds = request.FinishTimeSeconds,
            ResultStatus = request.ResultStatus,
            RecordedAt = DateTime.UtcNow
        };

        _db.Results.Add(result);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetEventResults), new { eventId }, new ResultResponse(
            result.ResultId,
            result.EnrolmentId,
            enrolment.ParticipantId,
            $"{enrolment.Participant.FirstName} {enrolment.Participant.LastName}",
            eventEntity.EventId,
            eventEntity.EventName,
            result.FinishPosition,
            result.FinishTimeSeconds,
            result.ResultStatus,
            result.RecordedAt));
    }

    [Authorize(Roles = "Organiser")]
    [HttpPut("api/results/{id:int}")]
    public async Task<ActionResult<ResultResponse>> Update(int id, ResultRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var result = await _db.Results
            .Include(x => x.Enrolment)
            .ThenInclude(x => x.Category)
            .ThenInclude(x => x.Event)
            .SingleOrDefaultAsync(x => x.ResultId == id);

        if (result is null)
            return NotFound();

        if (result.Enrolment.Category.Event.OrganiserId != User.UserId())
            return Forbid();

        if (request.ResultStatus is not ("Finished" or "DNF" or "DNS" or "Disqualified"))
            return BadRequest(new { message = "Result status must be Finished, DNF, DNS or Disqualified." });

        if (request.FinishPosition.HasValue && request.FinishPosition.Value <= 0)
            return BadRequest(new { message = "Finish position must be greater than zero." });

        if (request.FinishTimeSeconds.HasValue && request.FinishTimeSeconds.Value <= 0)
            return BadRequest(new { message = "Finish time must be greater than zero." });

        result.FinishPosition = request.FinishPosition;
        result.FinishTimeSeconds = request.FinishTimeSeconds;
        result.ResultStatus = request.ResultStatus;
        result.RecordedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new ResultResponse(
            result.ResultId,
            result.EnrolmentId,
            result.Enrolment.ParticipantId,
            $"{result.Enrolment.Participant.FirstName} {result.Enrolment.Participant.LastName}",
            result.Enrolment.Category.EventId,
            result.Enrolment.Category.Event.EventName,
            result.FinishPosition,
            result.FinishTimeSeconds,
            result.ResultStatus,
            result.RecordedAt));
    }

    [Authorize(Roles = "Organiser")]
    [HttpDelete("api/results/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _db.Results
            .Include(x => x.Enrolment)
            .ThenInclude(x => x.Category)
            .ThenInclude(x => x.Event)
            .SingleOrDefaultAsync(x => x.ResultId == id);

        if (result is null)
            return NotFound();

        if (result.Enrolment.Category.Event.OrganiserId != User.UserId())
            return Forbid();

        _db.Results.Remove(result);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
