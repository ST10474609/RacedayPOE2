using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    private readonly IPasswordService _passwords;

    public AuthController(RaceDayDbContext db, IPasswordService passwords)
    {
        _db = db;
        _passwords = passwords;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserResponse>> Register(RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var firstName = request.FirstName.Trim();
        var lastName = request.LastName.Trim();
        var email = request.Email.Trim();
        var role = request.Role.Trim();

        if (string.IsNullOrWhiteSpace(firstName) ||
            string.IsNullOrWhiteSpace(lastName) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "All required fields are required." });
        }

        if (!new EmailAddressAttribute().IsValid(email))
        {
            return BadRequest(new { message = "Email is invalid." });
        }

        if (request.Role is not "Organiser" and not "Participant")
        {
            return BadRequest(new { message = "Role must be Organiser or Participant." });
        }

        if (await _db.Users.AnyAsync(x => x.Email == email))
        {
            return Conflict(new { message = "An account with this email already exists." });
        }

        var user = new User
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PasswordHash = _passwords.Hash(request.Password),
            Role = role,
            PhoneNumber = request.PhoneNumber,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Created($"/api/users/{user.UserId}", new UserResponse(
            user.UserId,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Role,
            user.PhoneNumber,
            user.CreatedAt));
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserResponse>> Login(LoginRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var email = request.Email.Trim();
        var user = await _db.Users.SingleOrDefaultAsync(x => x.Email == email);

        if (user is null || !_passwords.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new(ClaimTypes.Role, user.Role),
            new("email", user.Email)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);

        HttpContext.Session.SetInt32("UserId", user.UserId);
        HttpContext.Session.SetString("Role", user.Role);

        return Ok(new UserResponse(
            user.UserId,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Role,
            user.PhoneNumber,
            user.CreatedAt));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }
}
