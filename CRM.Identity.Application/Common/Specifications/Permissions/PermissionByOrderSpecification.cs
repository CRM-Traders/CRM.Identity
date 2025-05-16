using CRM.Identity.Domain.Entities.Permissions;

namespace CRM.Identity.Application.Common.Specifications.Permissions;

public class PermissionByOrderSpecification(int order) : BaseSpecification<Permission>(p => p.Order == order);