using CRM.Identity.Domain.Entities.Permissions;
using CRM.Identity.Domain.Entities.Users.Enums;

namespace CRM.Identity.Application.Common.Specifications.Roles;

public sealed class RoleDefaultPermissionsByRoleSpecification : BaseSpecification<RoleDefaultPermission>
{
    public RoleDefaultPermissionsByRoleSpecification(Role role)
        : base(rdp => rdp.Role == role)
    {
        AddInclude(rdp => rdp.Permission);
    }
}