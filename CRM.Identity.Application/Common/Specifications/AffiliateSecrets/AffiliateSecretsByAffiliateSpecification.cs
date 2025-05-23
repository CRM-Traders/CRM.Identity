using CRM.Identity.Domain.Entities.Affiliate;

namespace CRM.Identity.Application.Common.Specifications.AffiliateSecrets;

public sealed class AffiliateSecretsByAffiliateSpecification : BaseSpecification<AffiliateSecret>
{
    public AffiliateSecretsByAffiliateSpecification(Guid affiliateId,bool includeAffiliate = false)
        : base(s => s.AffiliateId == affiliateId)
    {
        ApplyOrderByDescending(s => s.CreatedAt);
        if (includeAffiliate)
        {
            AddInclude("Affiliate");
        }
    }
}