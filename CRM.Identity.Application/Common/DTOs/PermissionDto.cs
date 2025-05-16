namespace CRM.Identity.Application.Common.DTOs;

public class PermissionDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public int Order { get; set; }
    public string? Description { get; set; }
}