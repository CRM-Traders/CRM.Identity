namespace CRM.Identity.Application.Features.Auth.Queries.IsAuthorized;

public sealed record IsAuthorizedQuery(): IRequest<Unit>;

public sealed class IsAuthorizedQueryHandler(IUserContext _userContext) : IRequestHandler<IsAuthorizedQuery, Unit>
{
    public async ValueTask<Result<Unit>> Handle(IsAuthorizedQuery request, CancellationToken cancellationToken)
    {
        if (_userContext.IsAuthenticated)
        {
            return Result.Success(Unit.Value);
        }

        return Result.Failure<Unit>("User is not authenticated.");
    }
}