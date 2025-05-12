namespace CRM.Identity.Application.Common.Services.Auth;

public interface IAuthenticationService
{
    Task<AuthenticationResult?> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<AuthenticationResult?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<bool> LogoutAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> InvalidateAllSessionsAsync(Guid userId, CancellationToken cancellationToken = default);
}
