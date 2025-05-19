namespace CRM.Identity.Application.Features.Users.Queries.UserSettings;

public sealed record UserSettingsQuery() : IRequest<UserSettingsQueryResponse>;

public sealed record UserSettingsQueryResponse(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    bool IsEmailConfirmed,
    string Role,
    bool IsTwoFactorenabled,
    bool IsTwoFactorverified);

public sealed class UserSettingsQueryHandler(IRepository<User> userRepository, IUserContext _userContext) : IRequestHandler<UserSettingsQuery, UserSettingsQueryResponse>
{
    public async ValueTask<Result<UserSettingsQueryResponse>> Handle(
        UserSettingsQuery request,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(_userContext.Id);

        if (user is null)
            return Result.Failure<UserSettingsQueryResponse>("Can't find user");

        var response = new UserSettingsQueryResponse(
            user.FirstName,
            user.LastName,
            user.Email,
            user.PhoneNumber,
            user.IsEmailConfirmed,
            user.Role.ToString(),
            user.IsTwoFactorEnabled,
            user.IsTwoFactorVerified);

        return Result.Success(response);
    }
}