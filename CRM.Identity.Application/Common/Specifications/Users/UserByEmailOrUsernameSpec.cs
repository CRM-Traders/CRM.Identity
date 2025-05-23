namespace CRM.Identity.Application.Common.Specifications.Users;

public sealed class UserByEmailOrUsernameSpec(string email, string username) : BaseSpecification<User>(u =>
    u.Email.ToLower() == email.ToLower() ||
    u.Username.ToLower() == username.ToLower());

public sealed class UserEmailOrUsernameSpec(string emailOrUsername) : BaseSpecification<User>(u =>
    u.Email.ToLower() == emailOrUsername.ToLower() ||
    u.Username.ToLower() == emailOrUsername.ToLower());