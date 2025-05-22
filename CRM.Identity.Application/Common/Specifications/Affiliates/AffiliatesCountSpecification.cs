using CRM.Identity.Domain.Entities.Affiliate;

namespace CRM.Identity.Application.Common.Specifications.Affiliates;

public class AffiliatesCountSpecification : BaseSpecification<Affiliate>
{
    public AffiliatesCountSpecification(string? searchTerm, bool? isActive)
    {
        Criteria = BuildCriteria(searchTerm, isActive);
    }

    private static Expression<Func<Affiliate, bool>> BuildCriteria(string? searchTerm, bool? isActive)
    {
        Expression<Func<Affiliate, bool>> criteria = a => true;

        if (!string.IsNullOrEmpty(searchTerm))
        {
            var term = searchTerm.ToLower();
            criteria = a => (a.Name.ToLower().Contains(term) || a.Email.ToLower().Contains(term));
        }

        if (isActive.HasValue)
        {
            var existingCriteria = criteria;
            criteria = a => existingCriteria.Compile()(a) && a.IsActive == isActive.Value;
        }

        return criteria;
    }
}