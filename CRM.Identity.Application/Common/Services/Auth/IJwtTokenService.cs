namespace CRM.Identity.Application.Common.Services.Auth;

public interface IJwtTokenService
{
    string GenerateJwtToken(User user);
    RefreshToken GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token, out SecurityToken? validatedToken);
}
