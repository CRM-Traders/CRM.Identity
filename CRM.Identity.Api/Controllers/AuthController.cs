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
    public async Task<IResult> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        return await SendAsync(command);
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> Logout()
    {
        return await SendAsync(new LogoutCommand());
    }

    [HttpPost("invalidate-all-sessions")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> InvalidateAllSessions()
    {
        return await SendAsync(new InvalidateAllSessionsCommand());
    }
}
