using DotNetCoreSqlDb.Domain.Entities;
using DotNetCoreSqlDb.Domain.Enums;
using DotNetCoreSqlDb.Infrastructure.Security;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace DotNetCoreSqlDb.Data;

public static class DevelopmentDatabaseInitializer
{
    public static async Task InitializeAsync(
        IServiceProvider services,
        IConfiguration configuration,
        ILogger logger)
    {
        var connectionString = DevelopmentConnectionStringSelector.GetDevelopmentConnectionString(configuration);

        if (!DevelopmentConnectionStringSelector.IsLocalConnectionString(connectionString))
        {
            logger.LogWarning(
                "Skipping development database migration and seed because the configured connection string is not local.");
            return;
        }

        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
        var tokenGenerator = scope.ServiceProvider.GetRequiredService<ITokenGenerator>();

        try
        {
            if (!dbContext.Database.GetMigrations().Any())
            {
                logger.LogWarning(
                    "Skipping local database migration and seed because no EF Core migrations exist yet for AppDbContext. Create and apply the first migration, then restart the app.");
                return;
            }

            await dbContext.Database.MigrateAsync();
        }
        catch (SqlException exception)
        {
            throw new InvalidOperationException(
                "The local development database could not be initialized. Start SQL Server LocalDB, or override ConnectionStrings:LocalMyDbConnection with another local SQL Server instance.",
                exception);
        }

        if (await dbContext.Users.AnyAsync())
        {
            return;
        }

        foreach (var user in CreateSeedUsers())
        {
            dbContext.Users.Add(user);
        }

        foreach (var identity in CreateSeedIdentities())
        {
            dbContext.AuthIdentities.Add(identity);
        }

        foreach (var credential in CreateSeedCredentials(passwordHasher))
        {
            dbContext.PasswordCredentials.Add(credential);
        }

        foreach (var session in CreateSeedSessions(tokenGenerator))
        {
            dbContext.AuthSessions.Add(session);
        }

        foreach (var token in CreateSeedEmailVerificationTokens(tokenGenerator))
        {
            dbContext.EmailVerificationTokens.Add(token);
        }

        foreach (var token in CreateSeedPasswordResetTokens(tokenGenerator))
        {
            dbContext.PasswordResetTokens.Add(token);
        }

        await dbContext.SaveChangesAsync();

        logger.LogInformation(
            "Seeded local development database with sample account data. Test users: alice.local@example.com / P@ssword123! and bob.local@example.com / P@ssword123!. Raw dev tokens: verify=VERIFY-BOB-LOCAL-2026 reset=RESET-ALICE-LOCAL-2026.");
    }

    private static IEnumerable<User> CreateSeedUsers()
    {
        var alice = new User
        {
            Id = SeedIds.AliceUserId,
            Email = "alice.local@example.com",
            EmailNormalized = "ALICE.LOCAL@EXAMPLE.COM",
            DisplayName = "Alice Local",
            Status = AccountStatus.Active,
            EmailVerifiedAt = new DateTime(2026, 2, 12, 9, 30, 0, DateTimeKind.Utc),
            CreatedAt = new DateTime(2026, 2, 10, 8, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 2, 20, 16, 45, 0, DateTimeKind.Utc)
        };

        var bob = new User
        {
            Id = SeedIds.BobUserId,
            Email = "bob.local@example.com",
            EmailNormalized = "BOB.LOCAL@EXAMPLE.COM",
            DisplayName = "Bob Local",
            Status = AccountStatus.Active,
            EmailVerifiedAt = null,
            CreatedAt = new DateTime(2026, 2, 11, 10, 15, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 2, 21, 11, 0, 0, DateTimeKind.Utc)
        };

        return
        [
            alice,
            bob
        ];
    }

    private static IEnumerable<AuthIdentity> CreateSeedIdentities()
    {
        return
        [
            new AuthIdentity
            {
                Id = SeedIds.AliceIdentityId,
                UserId = SeedIds.AliceUserId,
                Provider = AuthConstants.PasswordProvider,
                ProviderSubject = SeedIds.AliceUserId.ToString("N"),
                LinkedAt = new DateTime(2026, 2, 10, 8, 5, 0, DateTimeKind.Utc),
                LastUsedAt = new DateTime(2026, 2, 20, 16, 45, 0, DateTimeKind.Utc)
            },
            new AuthIdentity
            {
                Id = SeedIds.BobIdentityId,
                UserId = SeedIds.BobUserId,
                Provider = AuthConstants.PasswordProvider,
                ProviderSubject = SeedIds.BobUserId.ToString("N"),
                LinkedAt = new DateTime(2026, 2, 11, 10, 20, 0, DateTimeKind.Utc),
                LastUsedAt = null
            }
        ];
    }

    private static IEnumerable<PasswordCredential> CreateSeedCredentials(IPasswordHasher<User> passwordHasher)
    {
        var aliceReference = new User { Id = SeedIds.AliceUserId };
        var bobReference = new User { Id = SeedIds.BobUserId };

        return
        [
            new PasswordCredential
            {
                UserId = SeedIds.AliceUserId,
                PasswordHash = passwordHasher.HashPassword(aliceReference, "P@ssword123!"),
                PasswordChangedAt = new DateTime(2026, 2, 10, 8, 5, 0, DateTimeKind.Utc)
            },
            new PasswordCredential
            {
                UserId = SeedIds.BobUserId,
                PasswordHash = passwordHasher.HashPassword(bobReference, "P@ssword123!"),
                PasswordChangedAt = new DateTime(2026, 2, 11, 10, 20, 0, DateTimeKind.Utc)
            }
        ];
    }

    private static IEnumerable<AuthSession> CreateSeedSessions(ITokenGenerator tokenGenerator)
    {
        return
        [
            new AuthSession
            {
                Id = SeedIds.AliceSessionId,
                UserId = SeedIds.AliceUserId,
                TokenHash = tokenGenerator.ComputeSha256("ALICE-SESSION-LOCAL-2026"),
                CreatedAt = new DateTime(2026, 2, 18, 8, 0, 0, DateTimeKind.Utc),
                LastSeenAt = new DateTime(2026, 2, 20, 16, 45, 0, DateTimeKind.Utc),
                ExpiresAt = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
                RevokedAt = null
            },
            new AuthSession
            {
                Id = SeedIds.BobRevokedSessionId,
                UserId = SeedIds.BobUserId,
                TokenHash = tokenGenerator.ComputeSha256("BOB-REVOKED-SESSION-LOCAL-2026"),
                CreatedAt = new DateTime(2026, 2, 15, 9, 0, 0, DateTimeKind.Utc),
                LastSeenAt = new DateTime(2026, 2, 15, 12, 30, 0, DateTimeKind.Utc),
                ExpiresAt = new DateTime(2026, 3, 30, 0, 0, 0, DateTimeKind.Utc),
                RevokedAt = new DateTime(2026, 2, 16, 7, 0, 0, DateTimeKind.Utc)
            }
        ];
    }

    private static IEnumerable<EmailVerificationToken> CreateSeedEmailVerificationTokens(ITokenGenerator tokenGenerator)
    {
        return
        [
            new EmailVerificationToken
            {
                Id = SeedIds.BobVerificationTokenId,
                UserId = SeedIds.BobUserId,
                TokenHash = tokenGenerator.ComputeSha256("VERIFY-BOB-LOCAL-2026"),
                CreatedAt = new DateTime(2026, 2, 21, 11, 5, 0, DateTimeKind.Utc),
                ExpiresAt = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
                ConsumedAt = null
            }
        ];
    }

    private static IEnumerable<PasswordResetToken> CreateSeedPasswordResetTokens(ITokenGenerator tokenGenerator)
    {
        return
        [
            new PasswordResetToken
            {
                Id = SeedIds.AliceResetTokenId,
                UserId = SeedIds.AliceUserId,
                TokenHash = tokenGenerator.ComputeSha256("RESET-ALICE-LOCAL-2026"),
                CreatedAt = new DateTime(2026, 2, 22, 7, 0, 0, DateTimeKind.Utc),
                ExpiresAt = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
                ConsumedAt = null
            }
        ];
    }

    private static class SeedIds
    {
        public static readonly Guid AliceUserId = Guid.Parse("9bb8c796-8573-48b1-b338-4044248b0898");
        public static readonly Guid BobUserId = Guid.Parse("2a202b87-e886-461c-88e4-4caf11d95e4a");
        public static readonly Guid AliceIdentityId = Guid.Parse("ea28e099-d775-416b-873e-c233c1e4f6ad");
        public static readonly Guid BobIdentityId = Guid.Parse("36724757-3e84-4a3d-bb33-568f71b8480a");
        public static readonly Guid AliceSessionId = Guid.Parse("ea812420-97f0-44f3-b38d-d6ae63fd1177");
        public static readonly Guid BobRevokedSessionId = Guid.Parse("8dd0c0d4-eaf5-4093-a5c4-e66f871e1b7c");
        public static readonly Guid BobVerificationTokenId = Guid.Parse("d70035e0-f852-4d98-a9a3-a44218545a11");
        public static readonly Guid AliceResetTokenId = Guid.Parse("5b2f4d48-6c98-40a8-bba8-006ccfaabd07");
    }
}
