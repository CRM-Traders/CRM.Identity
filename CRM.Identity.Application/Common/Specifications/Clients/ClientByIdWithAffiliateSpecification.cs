using CRM.Identity.Domain.Entities.Clients;

namespace CRM.Identity.Application.Common.Specifications.Clients;

public sealed class ClientByIdWithAffiliateSpecification : BaseSpecification<Client>
{
    public ClientByIdWithAffiliateSpecification(Guid id)
        : base(c => c.Id == id)
    {
        AddInclude(c => c.Affiliate!);
    }
}