namespace CRM.Identity.Application.Features.Auth.Commands.Permissions;

public sealed record RevokePermissionCommand(Guid UserId, Guid PermissionId) : IRequest<Unit>;

public sealed class RevokePermissionCommandHandler(
    IPermissionService permissionService,
    IUnitOfWork unitOfWork) : IRequestHandler<RevokePermissionCommand, Unit>
{
    public async ValueTask<Result<Unit>> Handle(
        RevokePermissionCommand request,
        CancellationToken cancellationToken)
    {
        await permissionService.RevokePermissionAsync(
            request.UserId,
            request.PermissionId,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(Unit.Value);
    }
}