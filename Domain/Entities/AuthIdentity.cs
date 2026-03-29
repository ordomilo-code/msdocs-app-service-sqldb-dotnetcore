namespace DotNetCoreSqlDb.Domain.Entities;

public sealed class AuthIdentity
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Provider { get; set; } = string.Empty;

    public string ProviderSubject { get; set; } = string.Empty;

    public DateTime LinkedAt { get; set; }

    public DateTime? LastUsedAt { get; set; }

    public User User { get; set; } = null!;
}
