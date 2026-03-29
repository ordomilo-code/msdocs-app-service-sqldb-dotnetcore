using DotNetCoreSqlDb.Features.Auth.Requests;

namespace DotNetCoreSqlDb.Features.Auth;

public interface IAuthService
{
    Task RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);

    Task LoginWithPasswordAsync(
        PasswordLoginRequest request,
        HttpContext httpContext,
        CancellationToken cancellationToken);

    Task LogoutAsync(HttpContext httpContext, CancellationToken cancellationToken);

    Task SendVerificationEmailAsync(string email, CancellationToken cancellationToken);

    Task VerifyEmailAsync(string token, CancellationToken cancellationToken);

    Task ForgotPasswordAsync(string email, CancellationToken cancellationToken);

    Task ResetPasswordAsync(string token, string newPassword, CancellationToken cancellationToken);
}
