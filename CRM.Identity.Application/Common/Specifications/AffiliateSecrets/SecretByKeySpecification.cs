using CRM.Identity.Domain.Entities.Affiliate;

namespace CRM.Identity.Application.Common.Specifications.AffiliateSecrets;

public sealed class SecretByKeySpecification : BaseSpecification<AffiliateSecret>
{
    public SecretByKeySpecification(string secretKey) : base(x => x.SecretKey == secretKey)
    {
        AddInclude(x => x.Affiliate!);
        AddInclude("Affiliate.User");
    }
}