using CRM.Identity.Domain.Entities.Clients.Enums;
using CRM.Identity.Domain.Entities.Leads;

namespace CRM.Identity.Application.Common.Specifications.Leads;

public class LeadsCountSpecification : BaseSpecification<Lead>
{
    public LeadsCountSpecification(
        string? searchTerm,
        ClientStatus? status,
        bool? isProblematic,
        string? country,
        string? source)
    {
        Criteria = BuildCriteria(searchTerm, status, isProblematic, country, source);
    }

    private static Expression<Func<Lead, bool>> BuildCriteria(
        string? searchTerm,
        ClientStatus? status,
        bool? isProblematic,
        string? country,
        string? source)
    {
        return l => (string.IsNullOrEmpty(searchTerm) ||
                     l.FirstName.ToLower().Contains(searchTerm.ToLower()) ||
                     l.LastName.ToLower().Contains(searchTerm.ToLower()) ||
                     l.Email.ToLower().Contains(searchTerm.ToLower())) &&
                    (!status.HasValue || l.Status == status.Value) &&
                    (!isProblematic.HasValue || l.IsProblematic == isProblematic.Value) &&
                    (string.IsNullOrEmpty(country) ||
                     (l.Country != null && l.Country.ToLower().Contains(country.ToLower()))) &&
                    (string.IsNullOrEmpty(source) ||
                     (l.Source != null && l.Source.ToLower().Contains(source.ToLower())));
    }
}