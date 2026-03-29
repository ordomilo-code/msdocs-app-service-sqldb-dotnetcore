using System.ComponentModel.DataAnnotations;

namespace DotNetCoreSqlDb.Features.Auth.Requests;

public sealed class RegisterRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(320)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(128)]
    public string Password { get; init; } = string.Empty;

    [MaxLength(150)]
    public string? DisplayName { get; init; }
}

public sealed class PasswordLoginRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(320)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(128)]
    public string Password { get; init; } = string.Empty;
}

public sealed class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(320)]
    public string Email { get; init; } = string.Empty;
}

public sealed class ResetPasswordRequest
{
    [Required]
    [MaxLength(256)]
    public string Token { get; init; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(128)]
    public string NewPassword { get; init; } = string.Empty;
}

public sealed class SendVerificationEmailRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(320)]
    public string Email { get; init; } = string.Empty;
}

public sealed class VerifyEmailRequest
{
    [Required]
    [MaxLength(256)]
    public string Token { get; init; } = string.Empty;
}
