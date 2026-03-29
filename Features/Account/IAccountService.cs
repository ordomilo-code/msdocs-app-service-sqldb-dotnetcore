using DotNetCoreSqlDb.Features.Account.Requests;
using DotNetCoreSqlDb.Features.Account.Responses;

namespace DotNetCoreSqlDb.Features.Account;

public interface IAccountService
{
    Task<UserMeResponse> GetMeAsync(Guid userId, CancellationToken cancellationToken);

    Task UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyList<SessionResponse>> GetSessionsAsync(
        Guid userId,
        Guid? currentSessionId,
        CancellationToken cancellationToken);

    Task RevokeSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken);

    Task<IReadOnlyList<IdentityResponse>> GetIdentitiesAsync(Guid userId, CancellationToken cancellationToken);
}
