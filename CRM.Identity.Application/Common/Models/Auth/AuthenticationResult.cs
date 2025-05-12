namespace CRM.Identity.Application.Common.Models.Auth;

public record AuthenticationResult(string AccessToken, string RefreshToken, int Exp);
