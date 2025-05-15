namespace CRM.Identity.Infrastructure.Services.Auth;

public class AuthenticationService(
    IRepository<User> userRepository,
    IJwtTokenService jwtTokenService,
    IPasswordService passwordService,
    IRedisManager redisManager,
    ITotpService totpService,
    JwtOptions jwtOptions,
    ILogger<AuthenticationService> logger)
    : IAuthenticationService
{
    public async Task<AuthenticationResult?> LoginAsync(string email, string password, string? twoFactorCode = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userSpecification = new UserByEmailSpecification(email.Trim().ToLower());
            var user = await userRepository.FirstOrDefaultAsync(userSpecification, cancellationToken);

            if (user == null)
            {
                logger.LogWarning("Login failed: User with email {Email} not found", email);
                return null;
            }

            var isPasswordValid = passwordService.VerifyPassword(
                password,
                user.PasswordHash,
                Convert.FromBase64String(user.PasswordSalt));

            if (!isPasswordValid)
            {
                logger.LogWarning("Login failed: Invalid password for user {Email}", email);
                return null;
            }
 
            if (user.IsTwoFactorEnabled && user.IsTwoFactorVerified)
            {
                if (string.IsNullOrEmpty(twoFactorCode))
                { 
                    return AuthenticationResult.TwoFactorRequired(user.Id);
                }

                if (!totpService.ValidateCode(user.TwoFactorSecret!, twoFactorCode))
                {
                    logger.LogWarning("Login failed: Invalid 2FA code for user {Email}", email);
                    return null;
                }
            }

            var accessToken = jwtTokenService.GenerateJwtToken(user);
            var refreshToken = jwtTokenService.GenerateRefreshToken();

            await redisManager.SetRefreshTokenAsync(
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

            await redisManager.SetAsync(
                $"user_session:{user.Id}",
                sessionData,
                TimeSpan.FromMinutes(jwtOptions.AccessTokenValidityInMinutes),
                cancellationToken);

            logger.LogInformation("User {Email} logged in successfully", email);

            return AuthenticationResult.Success(
                accessToken,
                refreshToken.Token,
                jwtOptions.AccessTokenValidityInMinutes * 60);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during login for user {Email}", email);
            return null;
        }
    }

    public async Task<AuthenticationResult?> LoginWithRecoveryCodeAsync(string email, string password,
        string recoveryCode, CancellationToken cancellationToken = default)
    {
        try
        {
            var userSpecification = new UserByEmailSpecification(email.Trim().ToLower());
            var user = await userRepository.FirstOrDefaultAsync(userSpecification, cancellationToken);

            if (user == null)
            {
                logger.LogWarning("Login failed: User with email {Email} not found", email);
                return null;
            }

            var isPasswordValid = passwordService.VerifyPassword(
                password,
                user.PasswordHash,
                Convert.FromBase64String(user.PasswordSalt));

            if (!isPasswordValid)
            {
                logger.LogWarning("Login failed: Invalid password for user {Email}", email);
                return null;
            }
 
            if (!user.UseRecoveryCode(recoveryCode))
            {
                logger.LogWarning("Login failed: Invalid recovery code for user {Email}", email);
                return null;
            }
 
            var accessToken = jwtTokenService.GenerateJwtToken(user);
            var refreshToken = jwtTokenService.GenerateRefreshToken();

            await redisManager.SetRefreshTokenAsync(
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

            await redisManager.SetAsync(
                $"user_session:{user.Id}",
                sessionData,
                TimeSpan.FromMinutes(jwtOptions.AccessTokenValidityInMinutes),
                cancellationToken);

            logger.LogInformation("User {Email} logged in successfully with recovery code", email);

            return AuthenticationResult.Success(
                accessToken,
                refreshToken.Token,
                jwtOptions.AccessTokenValidityInMinutes * 60);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during login with recovery code for user {Email}", email);
            return null;
        }
    }

    public async Task<AuthenticationResult?> RefreshTokenAsync(string refreshToken,
        CancellationToken cancellationToken = default)
    { 
        try
        {
            var claimsPrincipal = jwtTokenService.ValidateToken(refreshToken, out var validatedToken);

            if (claimsPrincipal == null || validatedToken == null)
            {
                logger.LogWarning("Refresh token failed: Invalid token format");
                return null;
            }

            var userIdClaim = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == "Uid");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                logger.LogWarning("Refresh token failed: Missing or invalid user ID claim");
                return null;
            }

            var storedRefreshToken = await redisManager.GetRefreshTokenAsync(userId, cancellationToken);

            if (string.IsNullOrEmpty(storedRefreshToken) || storedRefreshToken != refreshToken)
            {
                logger.LogWarning("Refresh token failed: Token mismatch or expired for user {UserId}", userId);
                return null;
            }

            var user = await userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                logger.LogWarning("Refresh token failed: User {UserId} not found", userId);
                return null;
            }

            var newAccessToken = jwtTokenService.GenerateJwtToken(user);
            var newRefreshToken = jwtTokenService.GenerateRefreshToken();

            await redisManager.SetRefreshTokenAsync(
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

            await redisManager.SetAsync(
                $"user_session:{user.Id}",
                sessionData,
                TimeSpan.FromMinutes(jwtOptions.AccessTokenValidityInMinutes),
                cancellationToken);

            logger.LogInformation("Refresh token successful for user {UserId}", userId);

            return AuthenticationResult.Success(
                newAccessToken,
                newRefreshToken.Token,
                jwtOptions.AccessTokenValidityInMinutes * 60);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during token refresh");
            return null;
        }
    }

    public async Task<bool> LogoutAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await redisManager.InvalidateRefreshTokenAsync(userId, cancellationToken);
            logger.LogInformation("User {UserId} logged out", userId);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during logout for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> InvalidateAllSessionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await redisManager.InvalidateUserSessionsAsync(userId, cancellationToken);
            logger.LogInformation("All sessions invalidated for user {UserId}", userId);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error invalidating all sessions for user {UserId}", userId);
            return false;
        }
    }
}