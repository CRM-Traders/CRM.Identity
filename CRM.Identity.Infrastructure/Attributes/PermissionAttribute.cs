using CRM.Identity.Domain.Entities.Permissions.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CRM.Identity.Infrastructure.Attributes;
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class PermissionAttribute : Attribute, IAsyncAuthorizationFilter
{
    public int Order { get; }
    public string Title { get; } = string.Empty;
    public string Section { get; } = string.Empty;
    public string? Description { get; }
    public ActionType ActionType { get; }
    public string AllowedRoles { get; }

    public PermissionAttribute(
        int order,
        string title,
        string section,
        ActionType actionType,
        string allowedRoles,
        string description)
    {
        Order = order;
        Title = title;
        Description = description;
        Section = section;
        ActionType = actionType;
        AllowedRoles = allowedRoles;
    }

    public PermissionAttribute(
        int order,
        string title,
        string section,
        ActionType actionType,
        string allowedRoles)
    {
        Order = order;
        Title = title;
        Section = section;
        ActionType = actionType;
        AllowedRoles = allowedRoles;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var userContext = context.HttpContext.RequestServices.GetRequiredService<IUserContext>();
        var permissionService = context.HttpContext.RequestServices.GetRequiredService<IPermissionService>();

        if (!userContext.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var hasPermission = await permissionService.HasPermissionAsync(
            userContext.Id,
            Section,
            Title,
            ActionType
        );

        if (!hasPermission)
        {
            context.Result = new ForbidResult();
        }
    }
}