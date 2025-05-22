using CRM.Identity.Domain.Entities.Clients;

namespace CRM.Identity.Application.Common.Specifications.Clients;

public class ClientByEmailExcludingIdSpecification(string email, Guid excludeId)
    : BaseSpecification<Client>(c => c.Email == email && c.Id != excludeId);