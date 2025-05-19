namespace CRM.Identity.Infrastructure.Contexts;

public class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public Guid Id => GetUserId();
    public string? UserName => httpContextAccessor.HttpContext?.User.Identity?.Name;

    public string? Email => httpContextAccessor.HttpContext?.User.Claims
        .FirstOrDefault(c => c.Type == "Email")?.Value;

    public string? IpAddress => httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    private Guid GetUserId()
    {
        var userIdClaim = httpContextAccessor.HttpContext?.User.Claims
            .FirstOrDefault(c =>
                string.Equals(c.Type, "Uid", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.Type, "sub", StringComparison.OrdinalIgnoreCase) ||
                c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }

        return Guid.Empty;
    }
}