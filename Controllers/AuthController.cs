using DotNetCoreSqlDb.Features.Auth;
using DotNetCoreSqlDb.Features.Auth.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotNetCoreSqlDb.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        await authService.RegisterAsync(request, cancellationToken);
        return Ok();
    }

    [HttpPost("login/password")]
    public async Task<IActionResult> LoginPassword(PasswordLoginRequest request, CancellationToken cancellationToken)
    {
        await authService.LoginWithPasswordAsync(request, HttpContext, cancellationToken);
        return Ok();
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await authService.LogoutAsync(HttpContext, cancellationToken);
        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("password/forgot")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        await authService.ForgotPasswordAsync(request.Email, cancellationToken);
        return Ok(new { message = "If the account exists, an email has been sent." });
    }

    [AllowAnonymous]
    [HttpPost("password/reset")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        await authService.ResetPasswordAsync(request.Token, request.NewPassword, cancellationToken);
        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("email/send-verification")]
    public async Task<IActionResult> SendVerificationEmail(
        SendVerificationEmailRequest request,
        CancellationToken cancellationToken)
    {
        await authService.SendVerificationEmailAsync(request.Email, cancellationToken);
        return Ok(new { message = "If the account exists, an email has been sent." });
    }

    [AllowAnonymous]
    [HttpPost("email/verify")]
    public async Task<IActionResult> VerifyEmail(VerifyEmailRequest request, CancellationToken cancellationToken)
    {
        await authService.VerifyEmailAsync(request.Token, cancellationToken);
        return Ok();
    }
}
