namespace CRM.Identity.Api.Common.Attributes;


[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class PermissionAttribute : Attribute 
{
    public int Order { get; }
    public string Title { get; } = string.Empty;
    public string Section { get; } = string.Empty;
    public string? Description { get; }
    public ActionType ActionType { get; }
    public Role AllowedRoles { get; } 

    public PermissionAttribute(
        int order,
        string title,
        string section,
        ActionType actionType,
        Role roles,
        string description)
    {
        Order = order;
        Title = title;
        Description = description;
        Section = section;
        ActionType = actionType;
        AllowedRoles = roles;
    }

    public PermissionAttribute(
        int order,
        string title,
        string section,
        ActionType actionType,
        Role roles)
    {
        Order = order;
        Title = title;
        Section = section;
        ActionType = actionType;
        AllowedRoles = roles;
    }
}
