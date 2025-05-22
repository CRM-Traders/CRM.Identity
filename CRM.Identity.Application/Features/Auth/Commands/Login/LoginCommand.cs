namespace CRM.Identity.Application.Features.Auth.Commands.Login;

public sealed record LoginCommand(
    string EmailOrUsername,
    string Password,
    string? TwoFactorCode = null,
    bool? RememberMe = null) : IRequest<AuthenticationResult>;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.EmailOrUsername)
            .NotEmpty()
            .WithMessage("Email or Username is required.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long.");

        When(x => !string.IsNullOrEmpty(x.TwoFactorCode), () =>
        {
            RuleFor(x => x.TwoFactorCode)
                .Length(6)
                .Matches("^[0-9]+$")
                .WithMessage("2FA code must be 6 digits.");
        });
    }
}

public sealed class LoginCommandHandler(
    IAuthenticationService authenticationService) : IRequestHandler<LoginCommand, AuthenticationResult>
{
    public async ValueTask<Result<AuthenticationResult>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var result = await authenticationService.LoginAsync(
            request.EmailOrUsername,
            request.Password,
            request.TwoFactorCode,
            cancellationToken);

        if (result == null)
        {
            return Result.Failure<AuthenticationResult>("Invalid credentials", "Unauthorized");
        }

        if (result.RequiresTwoFactor)
        {
            return Result.Success(result);
        }

        return Result.Success(result);
    }
}