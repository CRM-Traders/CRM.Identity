namespace CRM.Identity.Application.Common.Managers;

public interface IRedisManager
{
    Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task<bool> KeyExistsAsync(string key, CancellationToken cancellationToken = default);
    Task<bool> SetRefreshTokenAsync(Guid userId, string refreshToken, DateTimeOffset expiryTime, CancellationToken cancellationToken = default);
    Task<string?> GetRefreshTokenAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> InvalidateUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> InvalidateRefreshTokenAsync(Guid userId, CancellationToken cancellationToken = default);
}