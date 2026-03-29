using DotNetCoreSqlDb.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotNetCoreSqlDb.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<AuthIdentity> AuthIdentities => Set<AuthIdentity>();

    public DbSet<PasswordCredential> PasswordCredentials => Set<PasswordCredential>();

    public DbSet<AuthSession> AuthSessions => Set<AuthSession>();

    public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();

    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
