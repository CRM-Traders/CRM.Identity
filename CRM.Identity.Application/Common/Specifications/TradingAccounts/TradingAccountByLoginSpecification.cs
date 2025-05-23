using CRM.Identity.Domain.Entities.TradingAccounts;

namespace CRM.Identity.Application.Common.Specifications.TradingAccounts;

public class TradingAccountByLoginSpecification(string accountLogin)
    : BaseSpecification<TradingAccount>(t => t.AccountLogin == accountLogin);