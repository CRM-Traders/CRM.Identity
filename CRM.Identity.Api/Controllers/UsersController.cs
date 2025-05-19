using CRM.Identity.Application.Features.Users.Queries.UserSettings;

namespace CRM.Identity.Api.Controllers;

public class UsersController(IMediator _send) : BaseController(_send)
{
    [HttpPost("register")]
    [Permission(1, "Register User", SectionConstants.Users, ActionType.C, RoleConstants.AllExceptUser)]
    [ProducesResponseType(typeof(Unit), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IResult> Register([FromBody] RegisterCommand command, CancellationToken cancellationToken)
    {
        return await SendAsync(command, cancellationToken);
    }

    [HttpGet("settings")]
    [Authorize]
    [ProducesResponseType(typeof(UserSettingsQueryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IResult> Settings(CancellationToken cancellationToken)
    {
        return await SendAsync(new UserSettingsQuery(), cancellationToken);
    }
}
