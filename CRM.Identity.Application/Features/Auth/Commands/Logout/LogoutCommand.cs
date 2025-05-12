namespace CRM.Identity.Application.Features.Auth.Commands.Logout;

public sealed record LogoutCommand : IRequest<Unit>;

public sealed class LogoutCommandHandler(IAuthenticationService _authenticationService, IUserContext _userContext) : IRequestHandler<LogoutCommand, Unit>
{
    public async ValueTask<Result<Unit>> Handle(
        LogoutCommand request,
        CancellationToken cancellationToken)
    {
        if (!_userContext.IsAuthenticated)
        {
            return Result.Failure<Unit>("User is not authenticated", "Unauthorized");
        }

        var result = await _authenticationService.LogoutAsync(_userContext.Id, cancellationToken);

        if (!result)
        {
            return Result.Failure<Unit>("Failed to logout", "InternalServerError");
        }

        return Result.Success(Unit.Value);
    }
}