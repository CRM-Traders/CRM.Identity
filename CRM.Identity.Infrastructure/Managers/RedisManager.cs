namespace CRM.Identity.Infrastructure.Managers;

public class RedisManager : IRedisManager
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly ILogger<RedisManager> _logger;
    private const string RefreshTokenPrefix = "refresh_token:";
    private const string UserSessionPrefix = "user_session:";

    public RedisManager(
        IConnectionMultiplexer connectionMultiplexer,
        ILogger<RedisManager> logger,
        RedisOptions redisOptions)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _logger = logger;
    }

    public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _connectionMultiplexer.GetDatabase();
            var serializedValue = JsonSerializer.Serialize(value);
            return await db.StringSetAsync(key, serializedValue, expiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting value in Redis for key {Key}", key);
            return false;
        }
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _connectionMultiplexer.GetDatabase();
            var value = await db.StringGetAsync(key);

            if (value.IsNullOrEmpty)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(value!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting value from Redis for key {Key}", key);
            return default;
        }
    }

    public async Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _connectionMultiplexer.GetDatabase();
            return await db.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing key {Key} from Redis", key);
            return false;
        }
    }

    public async Task<bool> KeyExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _connectionMultiplexer.GetDatabase();
            return await db.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if key {Key} exists in Redis", key);
            return false;
        }
    }

    public Task<bool> SetRefreshTokenAsync(Guid userId, string refreshToken, DateTimeOffset expiryTime, CancellationToken cancellationToken = default)
    {
        var key = $"{RefreshTokenPrefix}{userId}";
        var expiry = expiryTime.DateTime - DateTime.UtcNow;
        return SetAsync(key, refreshToken, expiry, cancellationToken);
    }

    public Task<string?> GetRefreshTokenAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var key = $"{RefreshTokenPrefix}{userId}";
        return GetAsync<string>(key, cancellationToken);
    }

    public async Task<bool> InvalidateUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _connectionMultiplexer.GetDatabase();
            var sessionKey = $"{UserSessionPrefix}{userId}";
            var refreshTokenKey = $"{RefreshTokenPrefix}{userId}";

            var tasks = new Task<bool>[]
            {
                db.KeyDeleteAsync(sessionKey),
                db.KeyDeleteAsync(refreshTokenKey)
            };

            await Task.WhenAll(tasks);
            return tasks.All(t => t.Result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating sessions for user {UserId}", userId);
            return false;
        }
    }

    public Task<bool> InvalidateRefreshTokenAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var key = $"{RefreshTokenPrefix}{userId}";
        return RemoveAsync(key, cancellationToken);
    }
}