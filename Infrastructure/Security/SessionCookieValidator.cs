using DotNetCoreSqlDb.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace DotNetCoreSqlDb.Infrastructure.Security;

public static class SessionCookieValidator
{
    public static async Task ValidateAsync(CookieValidatePrincipalContext context)
    {
        var principal = context.Principal;
        var sessionId = principal?.GetCurrentSessionId();

        Guid userId;
        try
        {
            userId = principal?.GetRequiredUserId() ?? Guid.Empty;
        }
        catch (UnauthorizedAccessException)
        {
            userId = Guid.Empty;
        }

        if (!sessionId.HasValue || userId == Guid.Empty)
        {
            await RejectAsync(context);
            return;
        }

        var dbContext = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();

        var session = await dbContext.AuthSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == sessionId.Value, context.HttpContext.RequestAborted);

        var now = DateTime.UtcNow;
        if (session is null
            || session.UserId != userId
            || session.RevokedAt.HasValue
            || session.ExpiresAt <= now)
        {
            await RejectAsync(context);
        }
    }

    private static async Task RejectAsync(CookieValidatePrincipalContext context)
    {
        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync(AuthConstants.CookieScheme);
    }
}
