namespace CRM.Identity.Domain.Common.Attributes;


[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class PermissionAttribute : Attribute 
{
    public int Order { get; }
    public string Title { get; } = string.Empty;
    public string Section { get; } = string.Empty;
    public string? Description { get; }
    public ActionType ActionType { get; }
    public string AllowedRoles { get; } 

    public PermissionAttribute(
        int order,
        string title,
        string section,
        ActionType actionType,
        string allowedRoles,
        string description)
    {
        Order = order;
        Title = title;
        Description = description;
        Section = section;
        ActionType = actionType;
        AllowedRoles = allowedRoles;
    }

    public PermissionAttribute(
        int order,
        string title,
        string section,
        ActionType actionType,
        string allowedRoles)
    {
        Order = order;
        Title = title;
        Section = section;
        ActionType = actionType;
        AllowedRoles = allowedRoles;
    }
}
