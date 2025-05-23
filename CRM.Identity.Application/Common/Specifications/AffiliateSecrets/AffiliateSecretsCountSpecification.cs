using CRM.Identity.Domain.Entities.Affiliate;

namespace CRM.Identity.Application.Common.Specifications.AffiliateSecrets;

public class AffiliateSecretsCountSpecification : BaseSpecification<AffiliateSecret>
{
    public AffiliateSecretsCountSpecification(
        Guid? affiliateId,
        bool? isActive,
        bool? isExpired)
    {
        Criteria = BuildCriteria(affiliateId, isActive, isExpired);
    }

    private static Expression<Func<AffiliateSecret, bool>> BuildCriteria(
        Guid? affiliateId,
        bool? isActive,
        bool? isExpired)
    {
        return s => (!affiliateId.HasValue || s.AffiliateId == affiliateId.Value) &&
                    (!isActive.HasValue || s.IsActive == isActive.Value) &&
                    (!isExpired.HasValue ||
                     (isExpired.Value
                         ? DateTimeOffset.UtcNow > s.ExpirationDate
                         : DateTimeOffset.UtcNow <= s.ExpirationDate));
    }
}