using CRM.Identity.Domain.Entities.Affiliate;

namespace CRM.Identity.Application.Common.Specifications.AffiliateSecrets;

public sealed class AffiliateSecretsFilterSpecification : BaseSpecification<AffiliateSecret>
{
    public AffiliateSecretsFilterSpecification(
        Guid? affiliateId,
        bool? isActive,
        bool? isExpired,
        int pageNumber = 1,
        int pageSize = 10,
        bool includeAffiliate = false)
    {
        Criteria = BuildCriteria(affiliateId, isActive, isExpired);
        ApplyOrderByDescending(s => s.CreatedAt);
        if (includeAffiliate)
        {
            AddInclude("Affiliate");
        }

        if (pageNumber > 0 && pageSize > 0)
        {
            ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        }
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