using CRM.Identity.Domain.Entities.Permissions.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CRM.Identity.Infrastructure.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class PermissionAuthorizeAttribute(string title, string section, ActionType actionType)
    : Attribute, IAsyncAuthorizationFilter
{
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
            section,
            title,
            actionType
        );

        if (!hasPermission)
        {
            context.Result = new ForbidResult();
        }
    }
}