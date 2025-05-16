using CRM.Identity.Domain.Entities.Permissions.Enums;
using Microsoft.AspNetCore.Authorization;

namespace CRM.Identity.Infrastructure.Authorization;

public  class PermissionRequirement(string section, string title, ActionType actionType)
    : IAuthorizationRequirement
{
    public string Section { get; } = section;
    public string Title { get; } = title;
    public ActionType ActionType { get; } = actionType;
}

public class PermissionRequirementHandler(
    IPermissionService permissionService,
    IUserContext userContext)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (!userContext.IsAuthenticated)
        {
            return;
        }

        var hasPermission = await permissionService.HasPermissionAsync(
            userContext.Id,
            requirement.Section,
            requirement.Title,
            requirement.ActionType);

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}