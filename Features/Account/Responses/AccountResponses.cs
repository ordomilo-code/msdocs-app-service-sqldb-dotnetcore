namespace DotNetCoreSqlDb.Features.Account.Responses;

public sealed record UserMeResponse(
    Guid Id,
    string Email,
    string? DisplayName,
    bool EmailVerified,
    string Status);

public sealed record SessionResponse(
    Guid Id,
    DateTime CreatedAt,
    DateTime? LastSeenAt,
    DateTime ExpiresAt,
    bool Current,
    bool Revoked);

public sealed record IdentityResponse(
    string Provider,
    DateTime LinkedAt,
    DateTime? LastUsedAt);
