namespace CRM.Identity.Api.Common.Constants;

public static class RoleConstants
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string User = "User";
    public const string All = "Admin,Manager,User";
    public const string AllExceptAdmin = "Manager,User";
    public const string AllExceptManager = "Admin,User";
    public const string AllExceptUser = "Admin,Manager";
}
