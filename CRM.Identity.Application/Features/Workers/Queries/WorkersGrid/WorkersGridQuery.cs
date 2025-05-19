using CRM.Identity.Application.Common.Models.Grids;
using CRM.Identity.Application.Common.Services.Grids;

namespace CRM.Identity.Application.Features.Workers.Queries.WorkersGrid;

public sealed class WorkersGridQuery : GridQueryBase, IRequest<GridResponse<WorkersGridQueryDto>>
{
}

public sealed record WorkersGridQueryDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    string Role,
    bool IsEmailConfirmed,
    bool IsTwoFactorEnabled,
    bool IsTwoFactorVerified);

public sealed class WorkersGridQueryHandler(IRepository<User> _userRepository, IGridService _gridService) : IRequestHandler<WorkersGridQuery, GridResponse<WorkersGridQueryDto>>
{
    public async ValueTask<Result<GridResponse<WorkersGridQueryDto>>> Handle(WorkersGridQuery request, CancellationToken cancellationToken)
    {
        var query = _userRepository.GetQueryable();

        var result = await _gridService.ProcessGridQuery(
            query,
            request,
            user => new WorkersGridQueryDto(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.PhoneNumber,
                user.Role.ToString(),
                user.IsEmailConfirmed,
                user.IsTwoFactorEnabled,
                user.IsTwoFactorVerified),
            cancellationToken);

        return Result.Success(result);
    }
}
