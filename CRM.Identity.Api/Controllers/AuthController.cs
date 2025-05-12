namespace CRM.Identity.Api.Controllers;

public class AuthController(IMediator _sender) : BaseController(_sender)
{
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        return await SendAsync(command);
    }
}
