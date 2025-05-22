using CRM.Identity.Domain.Entities.Affiliate;

namespace CRM.Identity.Application.Common.Specifications.Affiliates;

public class AffiliateByEmailSpecification(string email) : BaseSpecification<Affiliate>(a => a.Email == email);