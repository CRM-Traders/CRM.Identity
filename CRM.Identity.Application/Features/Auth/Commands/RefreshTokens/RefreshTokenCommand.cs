namespace CRM.Identity.Application.Features.Auth.Commands.RefreshTokens;

public sealed record RefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<AuthenticationResult>;

public sealed record RefreshTokenResponse(string AccessToken, string RefreshToken, long Exp);

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token is required.");
    }
}

public sealed class RefreshTokenCommandHandler(IAuthenticationService _authenticationService)
    : IRequestHandler<RefreshTokenCommand, AuthenticationResult>
{
    public async ValueTask<Result<AuthenticationResult>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var result = await _authenticationService.RefreshTokenAsync(request.AccessToken, request.RefreshToken, cancellationToken);

        if (result == null)
        {
            return Result.Failure<AuthenticationResult>("Invalid or expired refresh token", "Unauthorized");
        }

        return Result.Success(result);
    }
}