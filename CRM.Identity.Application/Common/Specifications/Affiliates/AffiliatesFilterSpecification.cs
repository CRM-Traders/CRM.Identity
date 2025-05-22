using CRM.Identity.Domain.Entities.Affiliate;

namespace CRM.Identity.Application.Common.Specifications.Affiliates;

public class AffiliatesFilterSpecification : BaseSpecification<Affiliate>
{
    public AffiliatesFilterSpecification(string? searchTerm, bool? isActive, int pageNumber = 1, int pageSize = 10)
    {
        var criteria = BuildCriteria(searchTerm, isActive);
        Criteria = criteria;
        
        ApplyOrderByDescending(a => a.CreatedAt);
        
        if (pageNumber > 0 && pageSize > 0)
        {
            ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        }
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