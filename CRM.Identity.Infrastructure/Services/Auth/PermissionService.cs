using CRM.Identity.Application.Common.Specifications.Roles;
using CRM.Identity.Domain.Entities.Permissions.Enums;
using System.Collections;
using System.Text;

namespace CRM.Identity.Infrastructure.Services.Auth;

public class PermissionService(
    IRepository<Permission> permissionRepository,
    IRepository<UserPermission> userPermissionRepository,
    IRepository<RoleDefaultPermission> roleDefaultPermissionRepository,
    IRepository<User> userRepository,
    IHttpContextAccessor httpContextAccessor,
    ILogger<PermissionService> logger)
    : IPermissionService
{
    private Dictionary<string, int> _permissionIndexMap;
    private List<Permission> _allPermissions;

    public async Task<IEnumerable<Permission>> GetUserPermissionsAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return [];
        }

        var rolePermissions = await GetRoleDefaultPermissionsAsync(user.Role, cancellationToken);

        var userPermissionSpec = new UserPermissionsByUserIdSpecification(userId);
        var userPermissions = await userPermissionRepository.ListAsync(userPermissionSpec, cancellationToken);

        var activeUserPermissions = userPermissions
            .Where(up => up.IsGranted && !up.IsExpired())
            .Select(up => up.Permission);

        return rolePermissions.Union(activeUserPermissions).Distinct();
    }

    public async Task<IEnumerable<Permission>> GetRoleDefaultPermissionsAsync(
        CRM.Identity.Domain.Entities.Users.Enums.Role role,
        CancellationToken cancellationToken = default)
    {
        var spec = new RoleDefaultPermissionsByRoleSpecification(role);
        var roleDefaultPermissions = await roleDefaultPermissionRepository.ListAsync(spec, cancellationToken);

        return roleDefaultPermissions.Select(rdp => rdp.Permission);
    }

    public async Task<bool> HasPermissionAsync(Guid userId, string section, string title, ActionType actionType,
        CancellationToken cancellationToken = default)
    {
        var user = httpContextAccessor?.HttpContext?.User;
        if (user != null && user.Identity?.IsAuthenticated == true)
        {
            var permissionClaim = user.FindFirst("permission");
            if (permissionClaim != null)
            {
                return HasPermissionFromBinary(permissionClaim.Value, section, title, actionType);
            }
        }

        var userPermissions = await GetUserPermissionsAsync(userId, cancellationToken);
        return userPermissions.Any(p =>
            string.Equals(p.Section, section, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(p.Title, title, StringComparison.OrdinalIgnoreCase) &&
            p.ActionType == actionType);
    }

    public async Task GrantPermissionAsync(Guid userId, Guid permissionId, DateTimeOffset? expiresAt = null,
        CancellationToken cancellationToken = default)
    {
        var spec = new UserPermissionByUserAndPermissionSpecification(userId, permissionId);
        var existingPermission = await userPermissionRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (existingPermission != null)
        {
            existingPermission.Grant();
            existingPermission.SetExpiration(expiresAt);
            await userPermissionRepository.UpdateAsync(existingPermission, cancellationToken);
        }
        else
        {
            var newPermission = new UserPermission(userId, permissionId, true, expiresAt);
            await userPermissionRepository.AddAsync(newPermission, cancellationToken);
        }
    }

    public async Task RevokePermissionAsync(Guid userId, Guid permissionId,
        CancellationToken cancellationToken = default)
    {
        var spec = new UserPermissionByUserAndPermissionSpecification(userId, permissionId);
        var existingPermission = await userPermissionRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (existingPermission != null)
        {
            existingPermission.Revoke();
            await userPermissionRepository.UpdateAsync(existingPermission, cancellationToken);
        }
    }

    public string GeneratePermissionBinary(IEnumerable<Permission> userPermissions)
    {
        if (_allPermissions == null)
        {
            _allPermissions = permissionRepository.GetAllAsync()
                .GetAwaiter().GetResult()
                .OrderBy(p => p.Order)
                .ToList();

            _permissionIndexMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < _allPermissions.Count; i++)
            {
                var permission = _allPermissions[i];
                _permissionIndexMap[$"{permission.Section}:{permission.Title}:{permission.ActionType}"] = i;
            }
        }

        if (!_allPermissions.Any())
            return string.Empty;

        var userPermissionDict = userPermissions.ToDictionary(
            p => $"{p.Section}:{p.Title}:{p.ActionType}",
            StringComparer.OrdinalIgnoreCase);

        var sb = new StringBuilder(_allPermissions.Count);

        for (int i = 0; i < _allPermissions.Count; i++)
        {
            var permission = _allPermissions[i];
            var key = $"{permission.Section}:{permission.Title}:{permission.ActionType}";
            sb.Append(userPermissionDict.ContainsKey(key) ? '1' : '0');
        }

        return sb.ToString();
    }

    public string GeneratePermissionOrderString(IEnumerable<Permission> permissions)
    {
        string binaryString = GeneratePermissionBinary(permissions);
        byte[] bytes = Encoding.UTF8.GetBytes(binaryString);
        return Convert.ToBase64String(bytes);
    }

    private bool HasPermissionFromBinary(string permissionBinary, string section, string title, ActionType actionType)
    {
        if (string.IsNullOrEmpty(permissionBinary))
            return false;

        try
        {
            try
            {
                byte[] bytes = Convert.FromBase64String(permissionBinary);
                permissionBinary = Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                // ignored
            }

            string permissionKey = $"{section}:{title}:{actionType}";

            if (_permissionIndexMap == null || !_permissionIndexMap.TryGetValue(permissionKey, out int index))
            {
                if (_allPermissions == null)
                {
                    _allPermissions = permissionRepository.GetAllAsync()
                        .GetAwaiter().GetResult()
                        .OrderBy(p => p.Order)
                        .ToList();

                    _permissionIndexMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                    for (int i = 0; i < _allPermissions.Count; i++)
                    {
                        var permission = _allPermissions[i];
                        _permissionIndexMap[$"{permission.Section}:{permission.Title}:{permission.ActionType}"] = i;
                    }
                }

                if (!_permissionIndexMap.TryGetValue(permissionKey, out index))
                    return false;
            }

            if (index >= permissionBinary.Length)
                return false;

            return permissionBinary[index] == '1';
        }
        catch
        {
            return false;
        }
    }
}