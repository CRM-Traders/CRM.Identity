using CRM.Identity.Domain.Entities.Permissions;

namespace CRM.Identity.Application.Common.Specifications.Users;

public sealed class UserPermissionsByUserIdSpecification : BaseSpecification<UserPermission>
{
    public UserPermissionsByUserIdSpecification(Guid userId)
        : base(up => up.UserId == userId)
    {
        AddInclude(up => up.Permission);
    }
}

public sealed class UserPermissionByUserAndPermissionSpecification : BaseSpecification<UserPermission>
{
    public UserPermissionByUserAndPermissionSpecification(Guid userId, Guid permissionId)
        : base(up => up.UserId == userId && up.PermissionId == permissionId)
    {
        AddInclude(up => up.Permission);
    }
}