using CRM.Identity.Domain.Entities.TradingAccounts;

namespace CRM.Identity.Application.Common.Specifications.TradingAccounts;

public sealed class TradingAccountsByClientSpecification : BaseSpecification<TradingAccount>
{
    public TradingAccountsByClientSpecification(Guid clientId, bool includeClient = false)
        : base(t => t.ClientId == clientId)
    {
        ApplyOrderByDescending(t => t.CreatedAt);

        if (includeClient)
        {
            AddInclude(t => t.Client!);
        }
    }
}