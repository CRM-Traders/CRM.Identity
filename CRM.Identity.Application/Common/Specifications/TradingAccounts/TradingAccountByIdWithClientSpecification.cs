using CRM.Identity.Domain.Entities.TradingAccounts;

namespace CRM.Identity.Application.Common.Specifications.TradingAccounts;

public sealed class TradingAccountByIdWithClientSpecification : BaseSpecification<TradingAccount>
{
    public TradingAccountByIdWithClientSpecification(Guid id)
        : base(t => t.Id == id)
    {
        AddInclude(t => t.Client!);
    }
}