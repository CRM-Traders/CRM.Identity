namespace CRM.Identity.Application.Common.Models.Auth;

public sealed record AuthenticationResult(
    string? AccessToken,
    string? RefreshToken,
    int ExpiresIn)
{
    public bool RequiresTwoFactor { get; init; }
    public Guid? UserId { get; init; }
 
    public static AuthenticationResult TwoFactorRequired(Guid userId) =>
        new(null, null, 0)
        {
            RequiresTwoFactor = true,
            UserId = userId
        };
 
    public static AuthenticationResult Success(string accessToken, string refreshToken, int expiresIn) =>
        new(accessToken, refreshToken, expiresIn)
        {
            RequiresTwoFactor = false
        };
}