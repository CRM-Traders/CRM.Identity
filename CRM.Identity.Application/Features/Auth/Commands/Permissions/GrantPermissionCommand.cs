using Microsoft.Extensions.Logging;

namespace CRM.Identity.Application.Features.Auth.Commands.Permissions;

public sealed record GrantPermissionCommand(
    Guid UserId,
    Guid PermissionId,
    DateTimeOffset? ExpiresAt = null) : IRequest<Unit>;

public sealed class GrantPermissionCommandValidator : AbstractValidator<GrantPermissionCommand>
{
    public GrantPermissionCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.PermissionId)
            .NotEmpty()
            .WithMessage("Permission ID is required.");

        When(x => x.ExpiresAt.HasValue, () =>
        {
            RuleFor(x => x.ExpiresAt)
                .GreaterThan(DateTimeOffset.UtcNow)
                .WithMessage("Expiration date must be in the future.");
        });
    }
}

public sealed class GrantPermissionCommandHandler(
    IPermissionService permissionService,
    IUnitOfWork unitOfWork,
    ILogger<GrantPermissionCommandHandler> logger) : IRequestHandler<GrantPermissionCommand, Unit>
{
    public async ValueTask<Result<Unit>> Handle(
        GrantPermissionCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            await permissionService.GrantPermissionAsync(
                request.UserId,
                request.PermissionId,
                request.ExpiresAt,
                cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Permission {PermissionId} granted to user {UserId}",
                request.PermissionId,
                request.UserId);

            return Result.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error granting permission {PermissionId} to user {UserId}",
                request.PermissionId,
                request.UserId);
            return Result.Failure<Unit>("Failed to grant permission", "InternalServerError");
        }
    }
}