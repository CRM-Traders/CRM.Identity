namespace CRM.Identity.Application.Common.Models.Auth;

public sealed record AuthenticationResult(
    string? AccessToken,
    string? RefreshToken,
    long Exp)
{
    public bool RequiresTwoFactor { get; init; }

    public static AuthenticationResult TwoFactorRequired(Guid userId) =>
        new(null, null, 0)
        {
            RequiresTwoFactor = true
        };

    public static AuthenticationResult Success(string accessToken, string refreshToken, long exp) =>
        new(accessToken, refreshToken, exp)
        {
            RequiresTwoFactor = false
        };
}