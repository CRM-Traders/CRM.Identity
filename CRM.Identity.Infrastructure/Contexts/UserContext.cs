namespace CRM.Identity.Infrastructure.Contexts;

public class UserContext(IHttpContextAccessor _httpContextAccessor) : IUserContext
{
    public Guid Id => GetUserId();
    public string? UserName => _httpContextAccessor.HttpContext?.User?.Identity?.Name;
    public string? Email => _httpContextAccessor.HttpContext?.User?.Claims
        .FirstOrDefault(c => c.Type == "email")?.Value;
    public string? IpAddress => _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    private Guid GetUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.Claims
            .FirstOrDefault(c => c.Type == "sub" || c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }

        return IsAuthenticated ? Guid.Empty : Guid.Empty;
    }
}