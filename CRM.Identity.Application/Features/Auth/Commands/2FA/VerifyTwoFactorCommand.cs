using Microsoft.Extensions.Logging;

namespace CRM.Identity.Application.Features.Auth.Commands._2FA;

public sealed record VerifyTwoFactorCommand(string Code) : IRequest<VerifyTwoFactorResponse>;

public sealed class VerifyTwoFactorCommandValidator : AbstractValidator<VerifyTwoFactorCommand>
{
    public VerifyTwoFactorCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .Length(6)
            .Matches("^[0-9]+$")
            .WithMessage("Code must be 6 digits.");
    }
}

public sealed class VerifyTwoFactorCommandHandler(
    IRepository<User> userRepository,
    ITotpService totpService,
    IUserContext userContext,
    IUnitOfWork unitOfWork,
    ILogger<VerifyTwoFactorCommandHandler> logger) : IRequestHandler<VerifyTwoFactorCommand, VerifyTwoFactorResponse>
{
    public async ValueTask<Result<VerifyTwoFactorResponse>> Handle(
        VerifyTwoFactorCommand request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(userContext.Id, cancellationToken);

        if (user == null)
        {
            return Result.Failure<VerifyTwoFactorResponse>("User not found", "NotFound");
        }

        if (!user.IsTwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorSecret))
        {
            return Result.Failure<VerifyTwoFactorResponse>("2FA is not activated", "BadRequest");
        }

        if (!totpService.ValidateCode(user.TwoFactorSecret, request.Code))
        {
            return Result.Failure<VerifyTwoFactorResponse>("wrong code", "BadRequest");
        }

        var recoveryCodes = totpService.GenerateRecoveryCodes();
        user.VerifyTwoFactorAuthentication(recoveryCodes);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("2FA verified for user {UserId}", user.Id);

        return Result.Success(new VerifyTwoFactorResponse
        {
            IsVerified = true,
            RecoveryCodes = recoveryCodes
        });
    }
}

public sealed record VerifyTwoFactorResponse
{
    public bool IsVerified { get; init; }
    public List<string> RecoveryCodes { get; init; } = new();
}