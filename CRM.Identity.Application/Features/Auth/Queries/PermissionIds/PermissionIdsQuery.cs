namespace CRM.Identity.Application.Features.Auth.Queries.PermissionIds;

public sealed record PermissionIdsQuery(Guid UserId) : IRequest<List<Guid>>;

public sealed class PermissionIdsQueryHandler(IPermissionService permissionService) : IRequestHandler<PermissionIdsQuery, List<Guid>>
{
    public async ValueTask<Result<List<Guid>>> Handle(
        PermissionIdsQuery request,
        CancellationToken cancellationToken)
    {
        var permissions = await permissionService.GetUserPermissionsAsync(
            request.UserId,
            cancellationToken);
        var permissionIds = permissions.Select(p => p.Id).ToList();
        return Result.Success(permissionIds);
    }
}