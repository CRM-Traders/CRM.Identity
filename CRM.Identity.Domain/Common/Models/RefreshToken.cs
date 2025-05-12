namespace CRM.Identity.Domain.Common.Models;

public sealed record RefreshToken(string Token, DateTimeOffset ValidTill);
