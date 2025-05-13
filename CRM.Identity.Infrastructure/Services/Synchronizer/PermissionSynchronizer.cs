namespace CRM.Identity.Infrastructure.Services.Synchronizer;

public class PermissionSynchronizer(
    IRepository<Permission> _repository,
    IUnitOfWork _unitOfWork,
    ILogger<PermissionSynchronizer> _logger) : IPermissionSynchronizer
{
    public async Task SynchronizePermissionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting permissions synchronization");

            var controllerTypes = Assembly.GetEntryAssembly()!
                .GetTypes()
                .Where(type =>
                    !type.IsAbstract &&
                    type.IsPublic &&
                    (
                        type.Name.EndsWith("Controller") ||
                        (type.BaseType != null && type.BaseType.Name.EndsWith("Controller"))
                    ));

            var permissions = new List<(PermissionAttribute Attribute, MethodInfo Method)>();

            foreach (var controllerType in controllerTypes)
            {
                var methods = controllerType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .Where(m => m.GetCustomAttributes(typeof(PermissionAttribute), true).Any());

                foreach (var method in methods)
                {
                    var permissionAttribute = method.GetCustomAttribute<PermissionAttribute>();
                    if (permissionAttribute != null)
                    {
                        permissions.Add((permissionAttribute, method));
                    }
                }
            }

            _logger.LogInformation("Found {Count} permissions across controllers", permissions.Count);

            var existingPermissions = await _repository.GetAllAsync(cancellationToken);

            var addedCount = 0;
            var updatedCount = 0;

            foreach (var (attribute, method) in permissions)
            {
                var existingPermission = existingPermissions.FirstOrDefault(p =>
                    p.Section == attribute.Section &&
                    p.Title == attribute.Title &&
                    p.ActionType == attribute.ActionType);

                if (existingPermission == null)
                {
                    var newPermission = new Permission(
                        attribute.Order,
                        attribute.Title,
                        attribute.Section,
                        attribute.Description,
                        attribute.ActionType,
                        attribute.AllowedRoles);

                    await _repository.AddAsync(newPermission, cancellationToken);
                    addedCount++;
                }
                else
                {
                    if (existingPermission.Order != attribute.Order ||
                        existingPermission.AllowedRoles != attribute.AllowedRoles ||
                        existingPermission.Description != attribute.Description)
                    {
                        existingPermission.Update(
                            attribute.Order,
                            attribute.Title,
                            attribute.Section,
                            attribute.Description,
                            attribute.ActionType,
                            attribute.AllowedRoles);

                        await _repository.UpdateAsync(existingPermission, cancellationToken);
                        updatedCount++;
                    }
                }
            }

            if (addedCount > 0 || updatedCount > 0)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation("Permissions synchronization completed. Added: {AddedCount}, Updated: {UpdatedCount}",
                addedCount, updatedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error synchronizing permissions");
            throw;
        }
    }
}