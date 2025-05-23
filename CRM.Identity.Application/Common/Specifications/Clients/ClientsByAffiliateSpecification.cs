using CRM.Identity.Domain.Entities.Clients;

namespace CRM.Identity.Application.Common.Specifications.Clients;

public class ClientsByAffiliateSpecification(Guid affiliateId)
    : BaseSpecification<Client>(c => c.AffiliateId == affiliateId);