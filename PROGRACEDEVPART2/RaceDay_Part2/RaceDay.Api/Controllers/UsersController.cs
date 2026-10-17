using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Services;
namespace RaceDay.Api.Controllers;
[ApiController,Authorize]
[Route("api/users")]
public class UsersController(RaceDayDbContext db) : ControllerBase
{
    /// <summary>Returns the authenticated user's profile.</summary>
    [HttpGet("me")] public async Task<ActionResult<UserResponse>> Me(){var u=await db.Users.FindAsync(User.UserId());return u is null?NotFound():Ok(new UserResponse(u.UserId,u.FirstName,u.LastName,u.Email,u.Role,u.PhoneNumber,u.CreatedAt));}
    /// <summary>Updates the authenticated user's profile.</summary>
    [HttpPut("me")] public async Task<ActionResult<UserResponse>> Update(UpdateProfileRequest request){var id=User.UserId();var u=await db.Users.FindAsync(id);if(u is null)return NotFound();var email=request.Email.Trim().ToLowerInvariant();if(await db.Users.AnyAsync(x=>x.Email==email&&x.UserId!=id))return Conflict(new{message="An account with this email already exists."});u.FirstName=request.FirstName.Trim();u.LastName=request.LastName.Trim();u.Email=email;u.PhoneNumber=request.PhoneNumber?.Trim();await db.SaveChangesAsync();return Ok(new UserResponse(u.UserId,u.FirstName,u.LastName,u.Email,u.Role,u.PhoneNumber,u.CreatedAt));}
}
