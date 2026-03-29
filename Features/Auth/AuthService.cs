using System.Security.Claims;
using DotNetCoreSqlDb.Data;
using DotNetCoreSqlDb.Domain.Entities;
using DotNetCoreSqlDb.Domain.Enums;
using DotNetCoreSqlDb.Features.Auth.Requests;
using DotNetCoreSqlDb.Infrastructure.Email;
using DotNetCoreSqlDb.Infrastructure.Http;
using DotNetCoreSqlDb.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DotNetCoreSqlDb.Features.Auth;

public sealed class AuthService(
    AppDbContext dbContext,
    IPasswordHasher<User> passwordHasher,
    ITokenGenerator tokenGenerator,
    IEmailSender emailSender,
    TimeProvider timeProvider) : IAuthService
{
    public async Task RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();
        var emailNormalized = NormalizeEmail(email);

        if (await dbContext.Users.AnyAsync(x => x.EmailNormalized == emailNormalized, cancellationToken))
        {
            throw new ApiException(StatusCodes.Status409Conflict, "An account with this email already exists.");
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            EmailNormalized = emailNormalized,
            DisplayName = NormalizeDisplayName(request.DisplayName),
            Status = AccountStatus.Active,
            CreatedAt = now,
            UpdatedAt = now
        };

        var emailVerification = CreateEmailVerificationToken(user.Id, now);

        dbContext.Users.Add(user);
        dbContext.AuthIdentities.Add(new AuthIdentity
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Provider = AuthConstants.PasswordProvider,
            ProviderSubject = user.Id.ToString("N"),
            LinkedAt = now
        });
        dbContext.PasswordCredentials.Add(new PasswordCredential
        {
            UserId = user.Id,
            PasswordHash = passwordHasher.HashPassword(user, request.Password),
            PasswordChangedAt = now
        });
        dbContext.EmailVerificationTokens.Add(emailVerification.Entity);

        await dbContext.SaveChangesAsync(cancellationToken);
        await emailSender.SendVerificationEmailAsync(user.Email, emailVerification.RawToken, cancellationToken);
    }

    public async Task LoginWithPasswordAsync(
        PasswordLoginRequest request,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var emailNormalized = NormalizeEmail(request.Email);
        var now = timeProvider.GetUtcNow().UtcDateTime;

        var user = await dbContext.Users
            .Include(x => x.PasswordCredential)
            .Include(x => x.AuthIdentities)
            .FirstOrDefaultAsync(x => x.EmailNormalized == emailNormalized, cancellationToken);

        if (user is null || user.PasswordCredential is null || user.Status != AccountStatus.Active)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var verificationResult = passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordCredential.PasswordHash,
            request.Password);

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordCredential.PasswordHash = passwordHasher.HashPassword(user, request.Password);
            user.PasswordCredential.PasswordChangedAt = now;
        }

        var passwordIdentity = user.AuthIdentities.FirstOrDefault(x => x.Provider == AuthConstants.PasswordProvider);
        if (passwordIdentity is not null)
        {
            passwordIdentity.LastUsedAt = now;
        }

        var session = CreateSession(user.Id, now);
        dbContext.AuthSessions.Add(session);
        await dbContext.SaveChangesAsync(cancellationToken);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(AuthConstants.SubjectClaimType, user.Id.ToString()),
            new(AuthConstants.SessionIdClaimType, session.Id.ToString())
        };

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, AuthConstants.CookieScheme));
        var authenticationProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            IssuedUtc = now,
            ExpiresUtc = session.ExpiresAt
        };

        await httpContext.SignInAsync(
            AuthConstants.CookieScheme,
            principal,
            authenticationProperties);
    }

    public async Task LogoutAsync(HttpContext httpContext, CancellationToken cancellationToken)
    {
        var sessionId = httpContext.User.GetCurrentSessionId();
        if (sessionId.HasValue)
        {
            var session = await dbContext.AuthSessions.FirstOrDefaultAsync(x => x.Id == sessionId.Value, cancellationToken);
            if (session is not null && session.RevokedAt is null)
            {
                session.RevokedAt = timeProvider.GetUtcNow().UtcDateTime;
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        await httpContext.SignOutAsync(AuthConstants.CookieScheme);
    }

    public async Task SendVerificationEmailAsync(string email, CancellationToken cancellationToken)
    {
        var emailNormalized = NormalizeEmail(email);
        var user = await dbContext.Users
            .FirstOrDefaultAsync(x => x.EmailNormalized == emailNormalized, cancellationToken);

        if (user is null || user.EmailVerifiedAt.HasValue)
        {
            return;
        }

        var token = CreateEmailVerificationToken(user.Id, timeProvider.GetUtcNow().UtcDateTime);
        dbContext.EmailVerificationTokens.Add(token.Entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        await emailSender.SendVerificationEmailAsync(user.Email, token.RawToken, cancellationToken);
    }

    public async Task VerifyEmailAsync(string token, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var tokenHash = tokenGenerator.ComputeSha256(token);

        var emailToken = await dbContext.EmailVerificationTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(
                x => x.TokenHash == tokenHash && x.ConsumedAt == null,
                cancellationToken);

        if (emailToken is null || emailToken.ExpiresAt <= now)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Invalid or expired verification token.");
        }

        emailToken.ConsumedAt = now;
        emailToken.User.EmailVerifiedAt ??= now;
        emailToken.User.UpdatedAt = now;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ForgotPasswordAsync(string email, CancellationToken cancellationToken)
    {
        var emailNormalized = NormalizeEmail(email);
        var user = await dbContext.Users
            .FirstOrDefaultAsync(x => x.EmailNormalized == emailNormalized, cancellationToken);

        if (user is null)
        {
            return;
        }

        var token = CreatePasswordResetToken(user.Id, timeProvider.GetUtcNow().UtcDateTime);
        dbContext.PasswordResetTokens.Add(token.Entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        await emailSender.SendPasswordResetEmailAsync(user.Email, token.RawToken, cancellationToken);
    }

    public async Task ResetPasswordAsync(string token, string newPassword, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var tokenHash = tokenGenerator.ComputeSha256(token);

        var passwordResetToken = await dbContext.PasswordResetTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(
                x => x.TokenHash == tokenHash && x.ConsumedAt == null,
                cancellationToken);

        if (passwordResetToken is null || passwordResetToken.ExpiresAt <= now)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Invalid or expired password reset token.");
        }

        var credential = await dbContext.PasswordCredentials
            .FirstOrDefaultAsync(x => x.UserId == passwordResetToken.UserId, cancellationToken);

        if (credential is null)
        {
            throw new ApiException(StatusCodes.Status400BadRequest, "Password credentials are not configured for this account.");
        }

        credential.PasswordHash = passwordHasher.HashPassword(passwordResetToken.User, newPassword);
        credential.PasswordChangedAt = now;
        passwordResetToken.ConsumedAt = now;
        passwordResetToken.User.UpdatedAt = now;

        var activeSessions = await dbContext.AuthSessions
            .Where(x => x.UserId == passwordResetToken.UserId && x.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var session in activeSessions)
        {
            session.RevokedAt = now;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private AuthSession CreateSession(Guid userId, DateTime now)
    {
        var rawSessionToken = tokenGenerator.GenerateToken();

        return new AuthSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenGenerator.ComputeSha256(rawSessionToken),
            CreatedAt = now,
            LastSeenAt = now,
            ExpiresAt = now.AddDays(30)
        };
    }

    private CreatedToken<EmailVerificationToken> CreateEmailVerificationToken(Guid userId, DateTime now)
    {
        var rawToken = tokenGenerator.GenerateToken();
        return new CreatedToken<EmailVerificationToken>(
            rawToken,
            new EmailVerificationToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TokenHash = tokenGenerator.ComputeSha256(rawToken),
                CreatedAt = now,
                ExpiresAt = now.AddHours(24)
            });
    }

    private CreatedToken<PasswordResetToken> CreatePasswordResetToken(Guid userId, DateTime now)
    {
        var rawToken = tokenGenerator.GenerateToken();
        return new CreatedToken<PasswordResetToken>(
            rawToken,
            new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TokenHash = tokenGenerator.ComputeSha256(rawToken),
                CreatedAt = now,
                ExpiresAt = now.AddHours(2)
            });
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToUpperInvariant();
    }

    private static string? NormalizeDisplayName(string? displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return null;
        }

        return displayName.Trim();
    }

    private sealed record CreatedToken<TEntity>(string RawToken, TEntity Entity);
}
