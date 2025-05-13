namespace CRM.Identity.Api.Controllers;

public class AuthController(IMediator _sender) : BaseController(_sender)
{
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthorizationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        return await SendAsync(command, cancellationToken);
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
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
}
