using Microsoft.Extensions.Logging;

namespace CRM.Identity.Application.Features.Auth.Commands._2FA;

public sealed record DisableTwoFactorCommand(string Code) : IRequest<Unit>;

public sealed class DisableTwoFactorCommandValidator : AbstractValidator<DisableTwoFactorCommand>
{
    public DisableTwoFactorCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .Length(6)
            .Matches("^[0-9]+$")
            .WithMessage("Code must be 6 digits.");
    }
}

public sealed class DisableTwoFactorCommandHandler(
    IRepository<User> userRepository,
    ITotpService totpService,
    IUserContext userContext,
    IUnitOfWork unitOfWork,
    ILogger<DisableTwoFactorCommandHandler> logger) : IRequestHandler<DisableTwoFactorCommand, Unit>
{
    public async ValueTask<Result<Unit>> Handle(
        DisableTwoFactorCommand request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(userContext.Id, cancellationToken);

        if (user == null)
        {
            return Result.Failure<Unit>("User not found", "NotFound");
        }

        if (!user.IsTwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorSecret))
        {
            return Result.Failure<Unit>("2FA is not active", "BadRequest");
        }

        if (!totpService.ValidateCode(user.TwoFactorSecret, request.Code))
        {
            return Result.Failure<Unit>("Incorrect Code", "BadRequest");
        }

        user.DisableTwoFactorAuthentication();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("2FA disabled for user {UserId}", user.Id);

        return Result.Success(Unit.Value);
    }
}