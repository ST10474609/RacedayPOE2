using System.Security.Claims;
namespace RaceDay.Api.Services;
public static class ClaimsExtensions { public static int UserId(this ClaimsPrincipal user) => int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!); }
