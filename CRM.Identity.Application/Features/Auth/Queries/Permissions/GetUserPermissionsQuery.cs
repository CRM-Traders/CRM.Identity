using CRM.Identity.Application.Common.DTOs;

namespace CRM.Identity.Application.Features.Auth.Queries.Permissions;

public sealed record GetUserPermissionsQuery(Guid UserId) : IRequest<IEnumerable<PermissionDto>>;

public sealed class GetUserPermissionsQueryHandler(
    IPermissionService permissionService) : IRequestHandler<GetUserPermissionsQuery, IEnumerable<PermissionDto>>
{
    public async ValueTask<Result<IEnumerable<PermissionDto>>> Handle(
        GetUserPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        var permissions = await permissionService.GetUserPermissionsAsync(
            request.UserId,
            cancellationToken);

        var permissionDtos = permissions.Select(p => new PermissionDto
        {
            Id = p.Id,
            Title = p.Title,
            Section = p.Section,
            ActionType = p.ActionType.ToString(),
            Order = p.Order
        });

        return Result.Success(permissionDtos);
    }
}