using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
using RaceDay.Api.Services;
namespace RaceDay.Api.Controllers;
[ApiController]
[Route("api/auth")]
public class AuthController(RaceDayDbContext db,IPasswordService passwords) : ControllerBase
{
    /// <summary>Registers a new Organiser or Participant account.</summary>
    [HttpPost("register")]
    public async Task<ActionResult<UserResponse>> Register(RegisterRequest request) {
        var role=request.Role.Trim(); if(role is not("Organiser") and not("Participant")) return BadRequest(new{message="Role must be Organiser or Participant."});
        var email=request.Email.Trim().ToLowerInvariant(); if(await db.Users.AnyAsync(x=>x.Email==email)) return Conflict(new{message="An account with this email already exists."});
        var user=new User { FirstName=request.FirstName.Trim(),LastName=request.LastName.Trim(),Email=email,PasswordHash=passwords.Hash(request.Password),Role=role,PhoneNumber=request.PhoneNumber?.Trim() };
        db.Users.Add(user); await db.SaveChangesAsync(); return Created($"/api/users/{user.UserId}",Map(user));
    }
    /// <summary>Authenticates a user and creates the application session.</summary>
    [HttpPost("login")]
    public async Task<ActionResult<UserResponse>> Login(LoginRequest request) {
        var user=await db.Users.SingleOrDefaultAsync(x=>x.Email==request.Email.Trim().ToLowerInvariant());
        if(user is null || !passwords.Verify(request.Password,user.PasswordHash)) return Unauthorized(new{message="Invalid email or password."});
        var claims=new[]{new Claim(ClaimTypes.NameIdentifier,user.UserId.ToString()),new Claim(ClaimTypes.Name,$"{user.FirstName} {user.LastName}"),new Claim(ClaimTypes.Role,user.Role),new Claim(ClaimTypes.Email,user.Email)};
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,new ClaimsPrincipal(new ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme)));
        HttpContext.Session.SetInt32("UserId",user.UserId); HttpContext.Session.SetString("Role",user.Role);
        return Ok(Map(user));
    }
    /// <summary>Ends the current authenticated session.</summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(){HttpContext.Session.Clear();await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);return NoContent();}
    static UserResponse Map(User u)=>new(u.UserId,u.FirstName,u.LastName,u.Email,u.Role,u.PhoneNumber,u.CreatedAt);
}
