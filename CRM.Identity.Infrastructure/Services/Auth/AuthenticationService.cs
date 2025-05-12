namespace CRM.Identity.Infrastructure.Services.Auth;

public class AuthenticationService : IAuthenticationService
{
    private readonly IRepository<User> _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordService _passwordService;
    private readonly IRedisManager _redisManager;
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        IRepository<User> userRepository,
        IJwtTokenService jwtTokenService,
        IPasswordService passwordService,
        IRedisManager redisManager,
        JwtOptions jwtOptions,
        ILogger<AuthenticationService> logger)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _passwordService = passwordService;
        _redisManager = redisManager;
        _jwtOptions = jwtOptions;
        _logger = logger;
    }

    public async Task<AuthenticationResult?> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            var userSpecification = new UserByEmailSpecification(email.Trim().ToLower());
            var user = await _userRepository.FirstOrDefaultAsync(userSpecification, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("Login failed: User with email {Email} not found", email);
                return null;
            }

            var isPasswordValid = _passwordService.VerifyPassword(
                password,
                user.PasswordHash,
                Convert.FromBase64String(user.PasswordSalt));

            if (!isPasswordValid)
            {
                _logger.LogWarning("Login failed: Invalid password for user {Email}", email);
                return null;
            }

            var accessToken = _jwtTokenService.GenerateJwtToken(user);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            await _redisManager.SetRefreshTokenAsync(
                user.Id,
                refreshToken.Token,
                refreshToken.ValidTill,
                cancellationToken);

            var sessionData = new UserSessionData
            {
                UserId = user.Id,
                Email = user.Email,
                LastLogin = DateTimeOffset.UtcNow
            };

            await _redisManager.SetAsync(
                $"user_session:{user.Id}",
                sessionData,
                TimeSpan.FromMinutes(_jwtOptions.AccessTokenValidityInMinutes),
                cancellationToken);

            _logger.LogInformation("User {Email} logged in successfully", email);

            return new AuthenticationResult(
                accessToken,
                refreshToken.Token,
                _jwtOptions.AccessTokenValidityInMinutes * 60);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {Email}", email);
            return null;
        }
    }

    public async Task<AuthenticationResult?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var claimsPrincipal = _jwtTokenService.ValidateToken(refreshToken, out var validatedToken);

            if (claimsPrincipal == null || validatedToken == null)
            {
                _logger.LogWarning("Refresh token failed: Invalid token format");
                return null;
            }

            var userIdClaim = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == "Uid");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                _logger.LogWarning("Refresh token failed: Missing or invalid user ID claim");
                return null;
            }

            var storedRefreshToken = await _redisManager.GetRefreshTokenAsync(userId, cancellationToken);

            if (string.IsNullOrEmpty(storedRefreshToken) || storedRefreshToken != refreshToken)
            {
                _logger.LogWarning("Refresh token failed: Token mismatch or expired for user {UserId}", userId);
                return null;
            }

            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("Refresh token failed: User {UserId} not found", userId);
                return null;
            }

            var newAccessToken = _jwtTokenService.GenerateJwtToken(user);
            var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

            await _redisManager.SetRefreshTokenAsync(
                userId,
                newRefreshToken.Token,
                newRefreshToken.ValidTill,
                cancellationToken);

            var sessionData = new UserSessionData
            {
                UserId = user.Id,
                Email = user.Email,
                LastLogin = DateTimeOffset.UtcNow
            };

            await _redisManager.SetAsync(
                $"user_session:{user.Id}",
                sessionData,
                TimeSpan.FromMinutes(_jwtOptions.AccessTokenValidityInMinutes),
                cancellationToken);

            _logger.LogInformation("Refresh token successful for user {UserId}", userId);

            return new AuthenticationResult(
                newAccessToken,
                newRefreshToken.Token,
                _jwtOptions.AccessTokenValidityInMinutes * 60);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return null;
        }
    }

    public async Task<bool> LogoutAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _redisManager.InvalidateRefreshTokenAsync(userId, cancellationToken);
            _logger.LogInformation("User {UserId} logged out", userId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> InvalidateAllSessionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _redisManager.InvalidateUserSessionsAsync(userId, cancellationToken);
            _logger.LogInformation("All sessions invalidated for user {UserId}", userId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating all sessions for user {UserId}", userId);
            return false;
        }
    }
}
