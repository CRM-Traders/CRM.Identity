namespace CRM.Identity.Application.Features.Auth.Queries.IsAuthorized;

public sealed record IsAuthorizedQuery() : IRequest<Unit>;

public sealed class IsAuthorizedQueryHandler(IUserContext _userContext) : IRequestHandler<IsAuthorizedQuery, Unit>
{
    public ValueTask<Result<Unit>> Handle(IsAuthorizedQuery request, CancellationToken cancellationToken)
    {
        if (_userContext.IsAuthenticated)
        {
            return ValueTask.FromResult(Result.Success(Unit.Value));
        }

        return ValueTask.FromResult(Result.Failure<Unit>("User is not authenticated."));
    }
}