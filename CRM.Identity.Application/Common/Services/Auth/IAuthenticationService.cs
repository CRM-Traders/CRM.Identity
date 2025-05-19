namespace CRM.Identity.Application.Common.Services.Auth;

public interface IAuthenticationService
{
    Task<AuthenticationResult?> LoginAsync(
        string email,
        string password,
        string? twoFactorCode = null,
        CancellationToken cancellationToken = default);

    Task<AuthenticationResult?> LoginWithRecoveryCodeAsync(
        string email,
        string password,
        string recoveryCode,
        CancellationToken cancellationToken = default);

    Task<AuthenticationResult?> RefreshTokenAsync(
        string accessToken,
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task<bool> LogoutAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<bool> InvalidateAllSessionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}