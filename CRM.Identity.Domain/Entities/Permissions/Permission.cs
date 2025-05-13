namespace CRM.Identity.Domain.Entities.Permissions;

public class Permission : Entity
{
    public int Order { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ActionType ActionType { get; set; }
    public string AllowedRoles { get; set; }

    public Permission(
        int order,
        string title,
        string section,
        string? description,
        ActionType actionType,
        string allowedRoles)
    {
        Order = order;
        Title = title;
        Section = section;
        Description = description;
        ActionType = actionType;
        AllowedRoles = allowedRoles;
    }

    public void Update(
        int order,
        string title,
        string section,
        string? description,
        ActionType actionType,
        string allowedRoles)
    {
        Order = order;
        Title = title;
        Section = section;
        Description = description;
        ActionType = actionType;
        AllowedRoles = allowedRoles;
    }
}

