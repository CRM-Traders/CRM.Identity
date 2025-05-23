using CRM.Identity.Domain.Entities.Affiliate;

namespace CRM.Identity.Application.Common.Specifications.Affiliates;

public class AffiliateByEmailExcludingIdSpecification(string email, Guid excludeId)
    : BaseSpecification<Affiliate>(a => a.Email == email && a.Id != excludeId);