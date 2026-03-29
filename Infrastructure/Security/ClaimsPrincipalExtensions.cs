using System.Security.Claims;

namespace DotNetCoreSqlDb.Infrastructure.Security;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetRequiredUserId(this ClaimsPrincipal principal)
    {
        var subjectValue = principal.FindFirstValue(AuthConstants.SubjectClaimType);
        if (!Guid.TryParse(subjectValue, out var userId))
        {
            throw new UnauthorizedAccessException("Missing authenticated user.");
        }

        return userId;
    }

    public static Guid? GetCurrentSessionId(this ClaimsPrincipal principal)
    {
        var sessionValue = principal.FindFirstValue(AuthConstants.SessionIdClaimType);
        return Guid.TryParse(sessionValue, out var sessionId) ? sessionId : null;
    }
}
