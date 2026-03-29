namespace DotNetCoreSqlDb.Infrastructure.Email;

public sealed class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendVerificationEmailAsync(string email, string rawToken, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Verification email requested for {Email}. Local raw verification token: {Token}",
            email,
            rawToken);

        return Task.CompletedTask;
    }

    public Task SendPasswordResetEmailAsync(string email, string rawToken, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Password reset email requested for {Email}. Local raw reset token: {Token}",
            email,
            rawToken);

        return Task.CompletedTask;
    }
}
