namespace CRM.Identity.Application.Common.Services.Synchronizer;

public interface IPermissionSynchronizer
{
    Task SynchronizePermissionsAsync(CancellationToken cancellationToken = default);
}