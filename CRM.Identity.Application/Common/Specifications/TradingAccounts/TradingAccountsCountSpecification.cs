using CRM.Identity.Domain.Entities.TradingAccounts;
using CRM.Identity.Domain.Entities.TradingAccounts.Enums;

namespace CRM.Identity.Application.Common.Specifications.TradingAccounts;

public sealed class TradingAccountsCountSpecification : BaseSpecification<TradingAccount>
{
    public TradingAccountsCountSpecification(
        Guid? clientId,
        Currency? currency,
        bool? isDemo,
        string? searchTerm)
    {
        Criteria = BuildCriteria(clientId, currency, isDemo, searchTerm);
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