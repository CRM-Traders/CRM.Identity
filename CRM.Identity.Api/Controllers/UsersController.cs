namespace CRM.Identity.Api.Controllers;

public class UsersController(IMediator _send) : BaseController(_send)
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(Unit), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IResult> Register([FromBody] RegisterCommand command, CancellationToken cancellationToken)
    {
        return await SendAsync(command, cancellationToken);
    }
}
