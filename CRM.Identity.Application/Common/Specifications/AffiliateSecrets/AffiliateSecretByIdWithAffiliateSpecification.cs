using CRM.Identity.Domain.Entities.Affiliate;

namespace CRM.Identity.Application.Common.Specifications.AffiliateSecrets;

public sealed class AffiliateSecretByIdWithAffiliateSpecification : BaseSpecification<AffiliateSecret>
{
    public AffiliateSecretByIdWithAffiliateSpecification(Guid id)
        : base(s => s.Id == id)
    {
        AddInclude("Affiliate");
    }
}