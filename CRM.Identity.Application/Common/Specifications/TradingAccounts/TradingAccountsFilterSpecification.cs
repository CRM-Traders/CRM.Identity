using CRM.Identity.Domain.Entities.TradingAccounts;
using CRM.Identity.Domain.Entities.TradingAccounts.Enums;

namespace CRM.Identity.Application.Common.Specifications.TradingAccounts;

public sealed class TradingAccountsFilterSpecification : BaseSpecification<TradingAccount>
{
    public TradingAccountsFilterSpecification(
        Guid? clientId,
        Currency? currency,
        bool? isDemo,
        string? searchTerm,
        int pageNumber = 1,
        int pageSize = 10,
        bool includeClient = false)
    {
        Criteria = BuildCriteria(clientId, currency, isDemo, searchTerm);
        ApplyOrderByDescending(t => t.CreatedAt);
        
        if (includeClient)
        {
            AddInclude(t => t.Client!);
        }
        
        if (pageNumber > 0 && pageSize > 0)
        {
            ApplyPaging((pageNumber - 1) * pageSize, pageSize);
        }
    }

    private static Expression<Func<TradingAccount, bool>> BuildCriteria(
        Guid? clientId,
        Currency? currency,
        bool? isDemo,
        string? searchTerm)
    {
        return t => (!clientId.HasValue || t.ClientId == clientId.Value) &&
                    (!currency.HasValue || t.Currency == currency.Value) &&
                    (!isDemo.HasValue || t.IsDemo == isDemo.Value) &&
                    (string.IsNullOrEmpty(searchTerm) || 
                     t.AccountLogin.ToLower().Contains(searchTerm.ToLower()) ||
                     (t.Server != null && t.Server.ToLower().Contains(searchTerm.ToLower())));
    }
}