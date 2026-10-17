using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public UsersController(RaceDayDbContext db)
    {
        _db = db;
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserResponse>> Me()
    {
        var userId = User.UserId();

        if (userId == 0)
        {
            return Unauthorized();
        }

        var user = await _db.Users.FindAsync(userId);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(new UserResponse(
            user.UserId,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Role,
            user.PhoneNumber,
            user.CreatedAt));
    }

    [HttpPut("me")]
    public async Task<ActionResult<UserResponse>> Update(UpdateProfileRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var userId = User.UserId();

        if (userId == 0)
        {
            return Unauthorized();
        }

        var user = await _db.Users.FindAsync(userId);

        if (user is null)
        {
            return NotFound();
        }

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.Email = request.Email.Trim();
        user.PhoneNumber = request.PhoneNumber;

        if (await _db.Users.AnyAsync(x => x.Email == user.Email && x.UserId != userId))
        {
            return Conflict(new { message = "An account with this email already exists." });
        }

        await _db.SaveChangesAsync();

        return Ok(new UserResponse(
            user.UserId,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Role,
            user.PhoneNumber,
            user.CreatedAt));
    }
}
