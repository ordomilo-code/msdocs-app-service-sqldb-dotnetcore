using DotNetCoreSqlDb.Features.Account;
using DotNetCoreSqlDb.Features.Account.Requests;
using DotNetCoreSqlDb.Features.Account.Responses;
using DotNetCoreSqlDb.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotNetCoreSqlDb.Controllers;

[ApiController]
[Authorize]
[Route("api/account")]
public sealed class AccountController(IAccountService accountService) : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<UserMeResponse>> Me(CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        return Ok(await accountService.GetMeAsync(userId, cancellationToken));
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        await accountService.UpdateProfileAsync(userId, request, cancellationToken);
        return Ok();
    }

    [HttpGet("sessions")]
    public async Task<ActionResult<IReadOnlyList<SessionResponse>>> Sessions(CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        var currentSessionId = User.GetCurrentSessionId();
        return Ok(await accountService.GetSessionsAsync(userId, currentSessionId, cancellationToken));
    }

    [HttpDelete("sessions/{sessionId:guid}")]
    public async Task<IActionResult> RevokeSession(Guid sessionId, CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        await accountService.RevokeSessionAsync(userId, sessionId, cancellationToken);
        return Ok();
    }

    [HttpGet("identities")]
    public async Task<ActionResult<IReadOnlyList<IdentityResponse>>> Identities(CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();
        return Ok(await accountService.GetIdentitiesAsync(userId, cancellationToken));
    }
}
