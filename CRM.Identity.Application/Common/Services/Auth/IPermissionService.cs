using CRM.Identity.Domain.Entities.Permissions;
using CRM.Identity.Domain.Entities.Permissions.Enums;
using CRM.Identity.Domain.Entities.Users.Enums;

namespace CRM.Identity.Application.Common.Services.Auth;

public interface IPermissionService
{
    Task<IEnumerable<Permission>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IEnumerable<Permission>> GetRoleDefaultPermissionsAsync(CRM.Identity.Domain.Entities.Users.Enums.Role role,
        CancellationToken cancellationToken = default);

    Task<bool> HasPermissionAsync(Guid userId, string section, string title, ActionType actionType,
        CancellationToken cancellationToken = default);

    Task GrantPermissionAsync(Guid userId, Guid permissionId, DateTimeOffset? expiresAt = null,
        CancellationToken cancellationToken = default);

    Task RevokePermissionAsync(Guid userId, Guid permissionId, CancellationToken cancellationToken = default);
    string GeneratePermissionOrderString(IEnumerable<Permission> permissions);
    string GeneratePermissionBinary(IEnumerable<Permission> userPermissions); 
}