using CRM.Identity.Domain.Entities.Affiliate;

namespace CRM.Identity.Application.Common.Specifications.AffiliateSecrets;

public sealed class AffiliateByUserIdSpecification : BaseSpecification<Affiliate>
{
    public AffiliateByUserIdSpecification(Guid userId)
        : base(x => x.UserId == userId)
    {
        AddInclude(x => x.User!);
    }
}