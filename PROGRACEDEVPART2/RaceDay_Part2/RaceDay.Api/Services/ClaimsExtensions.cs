using System.Security.Claims;

namespace RaceDay.Api.Services;

public static class ClaimsExtensions
{
    public static int UserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : 0;
    }
}
