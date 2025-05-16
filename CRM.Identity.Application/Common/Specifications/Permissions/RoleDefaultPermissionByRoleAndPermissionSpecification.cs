using CRM.Identity.Domain.Entities.Permissions;
using UserRole = CRM.Identity.Domain.Entities.Users.Enums.Role;

namespace CRM.Identity.Application.Common.Specifications.Permissions;

public class RoleDefaultPermissionByRoleAndPermissionSpecification(UserRole role, Guid permissionId)
    : BaseSpecification<RoleDefaultPermission>(rdp => rdp.Role == role && rdp.PermissionId == permissionId);