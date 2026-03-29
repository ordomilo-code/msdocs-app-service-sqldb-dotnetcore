namespace DotNetCoreSqlDb.Domain.Entities;

public sealed class PasswordCredential
{
    public Guid UserId { get; set; }

    public string PasswordHash { get; set; } = string.Empty;

    public DateTime PasswordChangedAt { get; set; }

    public User User { get; set; } = null!;
}
