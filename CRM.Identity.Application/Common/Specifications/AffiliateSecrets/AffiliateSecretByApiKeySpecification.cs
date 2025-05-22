using CRM.Identity.Domain.Entities.Affiliate;

namespace CRM.Identity.Application.Common.Specifications.AffiliateSecrets;

public class AffiliateSecretByApiKeySpecification(string apiKey)
    : BaseSpecification<AffiliateSecret>(s => s.ApiKey == apiKey);