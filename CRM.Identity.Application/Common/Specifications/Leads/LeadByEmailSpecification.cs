using CRM.Identity.Domain.Entities.Leads;

namespace CRM.Identity.Application.Common.Specifications.Leads;

public class LeadByEmailSpecification(string email) : BaseSpecification<Lead>(l => l.Email == email);