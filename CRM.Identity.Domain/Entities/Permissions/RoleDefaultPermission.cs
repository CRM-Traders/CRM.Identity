namespace CRM.Identity.Domain.Entities.Permissions;

public class RoleDefaultPermission : Entity
{
    public Role Role { get; private set; }
    public Guid PermissionId { get; private set; }
    public Permission Permission { get; private set; } = null!;

    private RoleDefaultPermission()
    {
    }

    public RoleDefaultPermission(Role role, Guid permissionId)
    {
        Role = role;
        PermissionId = permissionId;
    }
}