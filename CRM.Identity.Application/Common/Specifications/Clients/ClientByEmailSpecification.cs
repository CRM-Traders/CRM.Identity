using CRM.Identity.Domain.Entities.Clients;

namespace CRM.Identity.Application.Common.Specifications.Clients;

public class ClientByEmailSpecification(string email) : BaseSpecification<Client>(c => c.Email == email);