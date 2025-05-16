namespace CRM.Identity.Infrastructure.Services.Synchronizer;

public class PermissionSynchronizer(
    IRepository<Permission> repository,
    IUnitOfWork unitOfWork,
    IPermissionService permissionService,
    ILogger<PermissionSynchronizer> logger)
    : IPermissionSynchronizer
{
    public async Task SynchronizePermissionsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Starting permissions synchronization");
 
            var controllerPermissions = CollectPermissionsFromControllers();
            logger.LogInformation("Found {Count} permissions across controllers", controllerPermissions.Count);
 
            var existingPermissions = await repository.GetAllAsync(cancellationToken);
             
            var usedOrderNumbers = new HashSet<int>(existingPermissions.Select(p => p.Order));
             
            var existingByKey = existingPermissions.ToDictionary(
                p => $"{p.Section}:{p.Title}:{p.ActionType}", 
                StringComparer.OrdinalIgnoreCase);

            int addedCount = 0;
            int updatedCount = 0;
             
            foreach (var (attribute, _) in controllerPermissions)
            {
                var key = $"{attribute.Section}:{attribute.Title}:{attribute.ActionType}";
                 
                if (existingByKey.TryGetValue(key, out var existingPermission))
                { 
                    if (existingPermission.Order != attribute.Order ||
                        existingPermission.AllowedRoles != attribute.AllowedRoles ||
                        existingPermission.Description != attribute.Description)
                    { 
                        var oldOrder = existingPermission.Order;
                         
                        int newOrder = attribute.Order;
                        
                        if (usedOrderNumbers.Contains(newOrder) && newOrder != oldOrder)
                        { 
                            newOrder = FindNextAvailableOrder(usedOrderNumbers);
                            logger.LogWarning(
                                "Order value {Order} already in use. Using {NewOrder} for {Permission}",
                                attribute.Order, newOrder, key);
                        }
                         
                        existingPermission.Update(
                            newOrder,
                            attribute.Title,
                            attribute.Section,
                            attribute.Description,
                            attribute.ActionType,
                            attribute.AllowedRoles);
                         
                        usedOrderNumbers.Remove(oldOrder);
                        usedOrderNumbers.Add(newOrder);
                         
                        await repository.UpdateAsync(existingPermission, cancellationToken);
                        await unitOfWork.SaveChangesAsync(cancellationToken);
                        
                        updatedCount++;
                    }
                }
                else
                { 
                    int orderToUse = attribute.Order;
                     
                    if (usedOrderNumbers.Contains(orderToUse))
                    { 
                        orderToUse = FindNextAvailableOrder(usedOrderNumbers);
                        logger.LogWarning(
                            "Order value {Order} already in use. Using {NewOrder} for {Permission}",
                            attribute.Order, orderToUse, key);
                    }
                     
                    usedOrderNumbers.Add(orderToUse);
                     
                    var newPermission = new Permission(
                        orderToUse,
                        attribute.Title,
                        attribute.Section,
                        attribute.Description,
                        attribute.ActionType,
                        attribute.AllowedRoles);
                     
                    await repository.AddAsync(newPermission, cancellationToken);
                    await unitOfWork.SaveChangesAsync(cancellationToken);
                    
                    addedCount++;
                }
            }
  
            logger.LogInformation("Permissions synchronization completed. Added: {AddedCount}, Updated: {UpdatedCount}",
                addedCount, updatedCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error synchronizing permissions");
            throw;
        }
    }
 
    private List<(PermissionAttribute Attribute, MethodInfo Method)> CollectPermissionsFromControllers()
    {
        var permissions = new List<(PermissionAttribute Attribute, MethodInfo Method)>();
        
        var controllerTypes = Assembly.GetEntryAssembly()!
            .GetTypes()
            .Where(type =>
                !type.IsAbstract &&
                type.IsPublic &&
                (
                    type.Name.EndsWith("Controller") ||
                    (type.BaseType != null && type.BaseType.Name.EndsWith("Controller"))
                ));

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
        
        return permissions;
    }
     
    private int FindNextAvailableOrder(HashSet<int> usedOrders)
    {
        int order = 1;
        while (usedOrders.Contains(order))
        {
            order++;
        }
        return order;
    }
}