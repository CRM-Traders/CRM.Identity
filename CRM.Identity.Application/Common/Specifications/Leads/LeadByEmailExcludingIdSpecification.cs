using CRM.Identity.Domain.Entities.Leads;

namespace CRM.Identity.Application.Common.Specifications.Leads;

public class LeadByEmailExcludingIdSpecification(string email, Guid excludeId)
    : BaseSpecification<Lead>(l => l.Email == email && l.Id != excludeId);