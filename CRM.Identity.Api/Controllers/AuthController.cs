using CRM.Identity.Application.Common.Models.Auth;
using CRM.Identity.Application.Features.Auth.Commands._2FA;
using CRM.Identity.Application.Features.Auth.Queries.IsAuthorized;

namespace CRM.Identity.Api.Controllers;

public class AuthController(IMediator _sender) : BaseController(_sender)
{
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthenticationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        return await SendAsync(command, cancellationToken);
    }

    [HttpPost("login-recovery")]
    [ProducesResponseType(typeof(AuthenticationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> LoginWithRecoveryCode([FromBody] UseRecoveryCodeCommand command, CancellationToken cancellationToken)
    {
        return await SendAsync(command, cancellationToken);
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthenticationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> RefreshToken([FromBody] RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        return await SendAsync(command, cancellationToken);
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> Logout(CancellationToken cancellationToken)
    {
        return await SendAsync(new LogoutCommand(), cancellationToken);
    }

    [HttpPost("invalidate-all-sessions")]
    [Permission(2, "Invalidate All Sessions", "Auth", ActionType.D, RoleConstants.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> InvalidateAllSessions(CancellationToken cancellationToken)
    {
        return await SendAsync(new InvalidateAllSessionsCommand(), cancellationToken);
    }
 
    [HttpPost("2fa/setup")]
    [Authorize]
    [ProducesResponseType(typeof(SetupTwoFactorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> SetupTwoFactor(CancellationToken cancellationToken)
    {
        return await SendAsync(new SetupTwoFactorCommand(), cancellationToken);
    }

    [HttpPost("2fa/verify")]
    [Authorize]
    [ProducesResponseType(typeof(VerifyTwoFactorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> VerifyTwoFactor([FromBody] VerifyTwoFactorCommand command, CancellationToken cancellationToken)
    {
        return await SendAsync(command, cancellationToken);
    }

    [HttpPost("2fa/disable")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> DisableTwoFactor([FromBody] DisableTwoFactorCommand command, CancellationToken cancellationToken)
    {
        return await SendAsync(command, cancellationToken);
    }

    [HttpGet("is-authorized")]
    [Authorize]
    [ProducesResponseType(typeof(Unit), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> IsAuthorized(CancellationToken cancellationToken) 
    {
        return await SendAsync(new IsAuthorizedQuery(), cancellationToken);
    }
}