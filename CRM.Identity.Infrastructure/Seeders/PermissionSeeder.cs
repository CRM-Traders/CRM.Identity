using CRM.Identity.Application.Common.Constants;
using CRM.Identity.Application.Common.Specifications.Permissions;
using CRM.Identity.Domain.Entities.Permissions.Enums;
using UserRole = CRM.Identity.Domain.Entities.Users.Enums.Role;

namespace CRM.Identity.Infrastructure.Seeders;

public class PermissionSeeder(
    IRepository<Permission> permissionRepository,
    IRepository<RoleDefaultPermission> roleDefaultPermissionRepository,
    IUnitOfWork unitOfWork)
{
    public async Task SeedDefaultPermissionsAsync(CancellationToken cancellationToken = default)
    {
        var adminPermissions = new List<Permission>
        {
            new(1, "Manage Users", "Users", "Full user management access", ActionType.C, RoleConstants.Admin),
            new(2, "View All Users", "Users", "View all users", ActionType.V, RoleConstants.Admin),
        };

        foreach (var permission in adminPermissions)
        {
            var existingPermission = await permissionRepository.FirstOrDefaultAsync(
                new PermissionByOrderSpecification(permission.Order),
                cancellationToken);

            if (existingPermission == null)
            {
                await permissionRepository.AddAsync(permission, cancellationToken);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var savedPermissions = await permissionRepository.GetAllAsync(cancellationToken);

        foreach (var permission in savedPermissions)
        {
            if (permission.AllowedRoles.Contains(RoleConstants.Admin))
            {
                var existingRolePermission = await roleDefaultPermissionRepository.FirstOrDefaultAsync(
                    new RoleDefaultPermissionByRoleAndPermissionSpecification(UserRole.Admin, permission.Id),
                    cancellationToken);

                if (existingRolePermission == null)
                {
                    var roleDefault = new RoleDefaultPermission(UserRole.Admin, permission.Id);
                    await roleDefaultPermissionRepository.AddAsync(roleDefault, cancellationToken);
                }
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}