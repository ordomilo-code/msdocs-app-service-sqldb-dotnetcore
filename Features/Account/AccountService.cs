using DotNetCoreSqlDb.Data;
using DotNetCoreSqlDb.Features.Account.Requests;
using DotNetCoreSqlDb.Features.Account.Responses;
using DotNetCoreSqlDb.Infrastructure.Http;
using Microsoft.EntityFrameworkCore;

namespace DotNetCoreSqlDb.Features.Account;

public sealed class AccountService(
    AppDbContext dbContext,
    TimeProvider timeProvider) : IAccountService
{
    public async Task<UserMeResponse> GetMeAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "Account was not found.");

        return new UserMeResponse(
            user.Id,
            user.Email,
            user.DisplayName,
            user.EmailVerifiedAt.HasValue,
            user.Status.ToString());
    }

    public async Task UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "Account was not found.");

        user.DisplayName = NormalizeDisplayName(request.DisplayName);
        user.UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SessionResponse>> GetSessionsAsync(
        Guid userId,
        Guid? currentSessionId,
        CancellationToken cancellationToken)
    {
        return await dbContext.AuthSessions
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new SessionResponse(
                x.Id,
                x.CreatedAt,
                x.LastSeenAt,
                x.ExpiresAt,
                currentSessionId.HasValue && x.Id == currentSessionId.Value,
                x.RevokedAt.HasValue))
            .ToListAsync(cancellationToken);
    }

    public async Task RevokeSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken)
    {
        var session = await dbContext.AuthSessions
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == sessionId, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "Session was not found.");

        if (session.RevokedAt is null)
        {
            session.RevokedAt = timeProvider.GetUtcNow().UtcDateTime;
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IReadOnlyList<IdentityResponse>> GetIdentitiesAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await dbContext.AuthIdentities
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Provider)
            .Select(x => new IdentityResponse(
                x.Provider,
                x.LinkedAt,
                x.LastUsedAt))
            .ToListAsync(cancellationToken);
    }

    private static string? NormalizeDisplayName(string? displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return null;
        }

        return displayName.Trim();
    }
}
