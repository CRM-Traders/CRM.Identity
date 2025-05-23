using CRM.Identity.Application.Features.Users.Commands.ChangePassword;
using CRM.Identity.Application.Features.Users.Commands.RegisterUser;
using CRM.Identity.Application.Features.Users.Commands.ImportUsers;
using CRM.Identity.Application.Features.Users.Queries.GenerateUserTemplate;
using CRM.Identity.Application.Features.Users.Queries.UserSettings;
using CRM.Identity.Infrastructure.Attributes;

namespace CRM.Identity.Api.Controllers;

public class UsersController(IMediator _send) : BaseController(_send)
{
    [HttpPost("register")]
    // [Permission(1, "Register User", SectionConstants.Users, ActionType.C, RoleConstants.AllExceptUser)]
    [ProducesResponseType(typeof(Unit), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IResult> Register([FromBody] RegisterUserCommand command, CancellationToken cancellationToken)
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

    [HttpPost("import")]
    [Permission(3, "Import Users", SectionConstants.Users, ActionType.C, RoleConstants.Admin)]
    [ProducesResponseType(typeof(ImportUsersResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> ImportUsers(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return Results.BadRequest("File is required");

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, cancellationToken);
        var fileContent = memoryStream.ToArray();

        return await SendAsync(new ImportUsersCommand(fileContent), cancellationToken);
    }

    [HttpGet("import-template")]
    // [Permission(4, "Download User Import Template", SectionConstants.Users, ActionType.V, "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> GetImportTemplate(CancellationToken cancellationToken)
    {
        var result = await _send.Send(new GenerateUserTemplateQuery(), cancellationToken);

        if (!result.IsSuccess)
            return ToResult(result);

        var fileName = $"user_import_template_{DateTime.UtcNow:yyyyMMdd}.xlsx";
        return Results.File(result.Value!, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(Unit), StatusCodes.Status200OK)]
    public async Task<IResult> ChangePassword([FromBody] ChangePasswordCommand request, CancellationToken cancellation)
    {
        return await SendAsync(request, cancellation);
    }
}