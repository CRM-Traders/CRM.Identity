using CRM.Identity.Domain.Entities.Affiliate;

namespace CRM.Identity.Application.Common.Specifications.AffiliateSecrets;

public class AffiliateSecretBySecretKeySpecification(string secretKey)
    : BaseSpecification<AffiliateSecret>(s => s.SecretKey == secretKey);