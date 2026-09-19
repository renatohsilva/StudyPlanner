using System.Security.Claims;

namespace StudyPlanner.Api.Common;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Usuário autenticado sem NameIdentifier no token.");

        return Guid.Parse(value);
    }
}
