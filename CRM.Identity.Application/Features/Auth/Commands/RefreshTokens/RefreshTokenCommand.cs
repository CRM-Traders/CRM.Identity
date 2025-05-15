namespace CRM.Identity.Application.Features.Auth.Commands.RefreshTokens;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<RefreshTokenResponse>;

public sealed record RefreshTokenResponse(string AccessToken, string RefreshToken, int Exp);

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
    : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    public async ValueTask<Result<RefreshTokenResponse>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var result = await _authenticationService.RefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (result == null)
        {
            return Result.Failure<RefreshTokenResponse>("Invalid or expired refresh token", "Unauthorized");
        }

        return Result.Success(new RefreshTokenResponse(
            result.AccessToken,
            result.RefreshToken,
            result.ExpiresIn));
    }
}