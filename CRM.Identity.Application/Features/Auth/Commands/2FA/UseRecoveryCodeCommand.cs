using Microsoft.Extensions.Logging;

namespace CRM.Identity.Application.Features.Auth.Commands._2FA;

public sealed record UseRecoveryCodeCommand(
    string Email,
    string Password,
    string RecoveryCode) : IRequest<AuthenticationResult>;

public sealed class UseRecoveryCodeCommandValidator : AbstractValidator<UseRecoveryCodeCommand>
{
    public UseRecoveryCodeCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Invalid email format.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long.");

        RuleFor(x => x.RecoveryCode)
            .NotEmpty()
            .Length(8)
            .Matches("^[A-Z0-9]+$")
            .WithMessage("Invalid recovery code format.");
    }
}

public sealed class UseRecoveryCodeCommandHandler(
    IAuthenticationService authenticationService,
    IRepository<User> userRepository,
    IUnitOfWork unitOfWork,
    ILogger<UseRecoveryCodeCommandHandler> logger) : IRequestHandler<UseRecoveryCodeCommand, AuthenticationResult>
{
    public async ValueTask<Result<AuthenticationResult>> Handle(
        UseRecoveryCodeCommand request,
        CancellationToken cancellationToken)
    {
        var result = await authenticationService.LoginWithRecoveryCodeAsync(
            request.Email,
            request.Password,
            request.RecoveryCode,
            cancellationToken);

        if (result == null)
        {
            return Result.Failure<AuthenticationResult>("Invalid credentials or recovery code", "Unauthorized");
        }

        logger.LogInformation("User {Email} logged in with recovery code", request.Email);

        return Result.Success(result);
    }
}