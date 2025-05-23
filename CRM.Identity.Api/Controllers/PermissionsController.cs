using CRM.Identity.Application.Common.DTOs;
using CRM.Identity.Application.Features.Auth.Commands.Permissions;
using CRM.Identity.Application.Features.Auth.Queries.AllPermissions;
using CRM.Identity.Application.Features.Auth.Queries.PermissionIds;
using CRM.Identity.Application.Features.Auth.Queries.Permissions;
using CRM.Identity.Infrastructure.Attributes;

namespace CRM.Identity.Api.Controllers;

public class PermissionsController(IMediator _send) : BaseController(_send)
{
    [HttpPost("grant")]
    [Permission(1, "Grant Permission", "Permissions", ActionType.C, RoleConstants.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IResult> GrantPermission(
        [FromBody] GrantPermissionCommand command, CancellationToken cancellationToken)
    {
        return await SendAsync(command, cancellationToken);
    }

    [HttpPost("revoke")]
    [Permission(2, "Revoke Permission", "Permissions", ActionType.D, RoleConstants.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IResult> RevokePermission(
        [FromBody] RevokePermissionCommand command, CancellationToken cancellationToken)
    {
        return await SendAsync(command, cancellationToken);
    }

    [HttpGet("user/{userId}")]
    [Permission(3, "View User Permissions", "Permissions", ActionType.V, RoleConstants.Admin)]
    [ProducesResponseType(typeof(IEnumerable<PermissionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IResult> GetUserPermissions(Guid userId, CancellationToken cancellationToken)
    {
        return await SendAsync(new GetUserPermissionsQuery(userId), cancellationToken);
    }

    [HttpGet("user/{userId}/ids")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IResult> GetUserPermissionIds(Guid userId, CancellationToken cancellationToken)
    {
        return await SendAsync(new PermissionIdsQuery(userId), cancellationToken);
    }

    [HttpGet("all")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<PermissionSectionDto>), StatusCodes.Status200OK)]
    //[Permission(4, "View All Permissions", "Permissions", ActionType.V, RoleConstants.Admin)]
    public async Task<IResult> GetAllPermissions([FromQuery] AllPermissionsQuery request, CancellationToken cancellationToken)
    {
        return await SendAsync(request, cancellationToken);
    }
}