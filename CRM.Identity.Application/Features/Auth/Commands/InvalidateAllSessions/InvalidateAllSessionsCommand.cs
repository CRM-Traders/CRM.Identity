namespace CRM.Identity.Application.Features.Auth.Commands.InvalidateAllSessions;

public sealed record InvalidateAllSessionsCommand : IRequest<Unit>;

public sealed class InvalidateAllSessionsCommandHandler(IAuthenticationService _authenticationService, IUserContext _userContext) : IRequestHandler<InvalidateAllSessionsCommand, Unit>
{
    public async ValueTask<Result<Unit>> Handle(
        InvalidateAllSessionsCommand request,
        CancellationToken cancellationToken)
    {
        if (!_userContext.IsAuthenticated)
        {
            return Result.Failure<Unit>("User is not authenticated", "Unauthorized");
        }

        var result = await _authenticationService.InvalidateAllSessionsAsync(_userContext.Id, cancellationToken);

        if (!result)
        {
            return Result.Failure<Unit>("Failed to invalidate all sessions", "InternalServerError");
        }

        return Result.Success(Unit.Value);
    }
}