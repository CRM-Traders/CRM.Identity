using CRM.Identity.Domain.Entities.Users;

namespace CRM.Identity.Domain.Entities.Permissions;

public class UserPermission : Entity
{
    public Guid UserId { get; private set; }
    public Guid PermissionId { get; private set; }
    public bool IsGranted { get; private set; }
    public DateTimeOffset GrantedAt { get; private set; }
    public DateTimeOffset? ExpiresAt { get; private set; }

    public User User { get; private set; } = null!;
    public Permission Permission { get; private set; } = null!;

    private UserPermission()
    {
    }

    public UserPermission(
        Guid userId,
        Guid permissionId,
        bool isGranted = true,
        DateTimeOffset? expiresAt = null)
    {
        UserId = userId;
        PermissionId = permissionId;
        IsGranted = isGranted;
        GrantedAt = DateTimeOffset.UtcNow;
        ExpiresAt = expiresAt;
    }

    public void Grant()
    {
        IsGranted = true;
        GrantedAt = DateTimeOffset.UtcNow;
    }

    public void Revoke()
    {
        IsGranted = false;
    }

    public void SetExpiration(DateTimeOffset? expiresAt)
    {
        ExpiresAt = expiresAt;
    }

    public bool IsExpired()
    {
        return ExpiresAt.HasValue && ExpiresAt.Value < DateTimeOffset.UtcNow;
    }
}