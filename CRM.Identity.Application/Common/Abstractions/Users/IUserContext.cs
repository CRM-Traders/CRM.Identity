namespace CRM.Identity.Application.Common.Abstractions.Users;

public interface IUserContext
{
    Guid Id { get; }
    string? UserName { get; }
    string? Email { get; }
    string? IpAddress { get; }
    bool IsAuthenticated { get; }
}