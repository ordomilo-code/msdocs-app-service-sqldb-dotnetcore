namespace DotNetCoreSqlDb.Infrastructure.Email;

public interface IEmailSender
{
    Task SendVerificationEmailAsync(string email, string rawToken, CancellationToken cancellationToken);

    Task SendPasswordResetEmailAsync(string email, string rawToken, CancellationToken cancellationToken);
}
