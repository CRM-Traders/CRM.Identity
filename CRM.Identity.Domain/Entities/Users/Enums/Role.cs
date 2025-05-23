namespace CRM.Identity.Domain.Entities.Users.Enums;

[Flags]
public enum Role
{
    None = 0,
    User = 1,
    Manager = 2,
    Admin = 3,
    SuperUser = 4,
    
}
