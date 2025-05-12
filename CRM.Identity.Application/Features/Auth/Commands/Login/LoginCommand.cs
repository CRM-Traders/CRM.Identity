namespace CRM.Identity.Application.Features.Auth.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<LoginResponse>;

public sealed record LoginResponse(string AccessToken, string RefreshToken, int Exp);

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

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IAuthenticationService _authenticationService;
    public LoginCommandHandler(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }
    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var result = await _authenticationService.LoginAsync(request.Email, request.Password);
        if (result == null)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }
        return new LoginResponse(result.AccessToken, result.RefreshToken, result.Exp);
    }
}