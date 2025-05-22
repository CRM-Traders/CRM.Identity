using CRM.Identity.Domain.Entities.Clients;
using CRM.Identity.Domain.Entities.Clients.Enums;

namespace CRM.Identity.Application.Common.Specifications.Clients;

public class ClientsCountSpecification : BaseSpecification<Client>
{
    public ClientsCountSpecification(
        string? searchTerm,
        Guid? affiliateId,
        ClientStatus? status,
        bool? isProblematic,
        bool? isBonusAbuser)
    {
        Criteria = BuildCriteria(searchTerm, affiliateId, status, isProblematic, isBonusAbuser);
    }

    private static Expression<Func<Client, bool>> BuildCriteria(
        string? searchTerm,
        Guid? affiliateId,
        ClientStatus? status,
        bool? isProblematic,
        bool? isBonusAbuser)
    {
        return c => (string.IsNullOrEmpty(searchTerm) || 
                     c.FirstName.ToLower().Contains(searchTerm.ToLower()) || 
                     c.LastName.ToLower().Contains(searchTerm.ToLower()) || 
                     c.Email.ToLower().Contains(searchTerm.ToLower())) &&
                    (!affiliateId.HasValue || c.AffiliateId == affiliateId.Value) &&
                    (!status.HasValue || c.Status == status.Value) &&
                    (!isProblematic.HasValue || c.IsProblematic == isProblematic.Value) &&
                    (!isBonusAbuser.HasValue || c.IsBonusAbuser == isBonusAbuser.Value);
    }
}