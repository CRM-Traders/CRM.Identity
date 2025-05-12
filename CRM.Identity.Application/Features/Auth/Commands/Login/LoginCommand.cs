namespace CRM.Identity.Application.Features.Auth.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<AuthenticationResult>;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand> 
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Invalid email format.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .WithMessage("Password must be at least 6 characters long.");
    }
}
public sealed class LoginCommandHandler(IAuthenticationService _authenticationService) : IRequestHandler<LoginCommand, AuthenticationResult>
{
    public async ValueTask<Result<AuthenticationResult>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var result = await _authenticationService.LoginAsync(
            request.Email,
            request.Password,
            cancellationToken);

        if (result == null)
        {
            return Result.Failure<AuthenticationResult>("Invalid email or password", "Unauthorized");
        }

        return Result.Success(result);
    }
}