using DotNetCoreSqlDb.Domain.Enums;

namespace DotNetCoreSqlDb.Domain.Entities;

public sealed class User
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string EmailNormalized { get; set; } = string.Empty;

    public string? DisplayName { get; set; }

    public AccountStatus Status { get; set; } = AccountStatus.Active;

    public DateTime? EmailVerifiedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public PasswordCredential? PasswordCredential { get; set; }

    public ICollection<AuthIdentity> AuthIdentities { get; set; } = [];

    public ICollection<AuthSession> AuthSessions { get; set; } = [];

    public ICollection<EmailVerificationToken> EmailVerificationTokens { get; set; } = [];

    public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = [];
}
