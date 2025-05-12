namespace CRM.Identity.Application.Common.Models.Auth;

public class UserSessionData
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTimeOffset LastLogin { get; set; }
}