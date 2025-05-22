using CRM.Identity.Domain.Entities.Permissions;
using CRM.Identity.Domain.Entities.Permissions.Enums;
using Microsoft.EntityFrameworkCore;

namespace CRM.Identity.Application.Features.Auth.Queries.AllPermissions;

public sealed record AllPermissionsQuery(string? Role) : IRequest<List<PermissionSectionDto>>;

public sealed record PermissionSectionDto(string Section, List<PermissionListDto> Permissions);
public sealed record PermissionListDto(
    Guid Id,
    string Name,
    string? Description,
    ActionType actionType);

public sealed class AllPermissionsQueryHandler(IRepository<Permission> _permissionRepository) : IRequestHandler<AllPermissionsQuery, List<PermissionSectionDto>>
{
    public async ValueTask<Result<List<PermissionSectionDto>>> Handle(AllPermissionsQuery request, CancellationToken cancellationToken)
    {
        var permissions = _permissionRepository.GetQueryable();

        if (!string.IsNullOrEmpty(request.Role)) 
        {
            permissions = permissions.Where(x => x.AllowedRoles.Contains(request.Role));
        }

        var result = await permissions
            .GroupBy(x => x.Section, x => new PermissionListDto(
                x.Id,
                x.Title,
                x.Description,
                x.ActionType))
            .Select(x => new PermissionSectionDto(x.Key, x.ToList()))
            .ToListAsync(cancellationToken);

        return Result.Success(result);
    }
}
