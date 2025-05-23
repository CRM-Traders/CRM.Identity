using CRM.Identity.Application.Common.Specifications.TradingAccounts;
using CRM.Identity.Application.Features.TradingAccounts.Queries.GetTradingAccounts;
using CRM.Identity.Domain.Entities.TradingAccounts;

namespace CRM.Identity.Application.Features.TradingAccounts.Queries.GetTradingAccountsByClient;

public sealed record GetTradingAccountsByClientQuery(Guid ClientId) : IRequest<List<TradingAccountDto>>;

public sealed class GetTradingAccountsByClientQueryHandler(
    IRepository<TradingAccount> tradingAccountRepository)
    : IRequestHandler<GetTradingAccountsByClientQuery, List<TradingAccountDto>>
{
    public async ValueTask<Result<List<TradingAccountDto>>> Handle(GetTradingAccountsByClientQuery request,
        CancellationToken cancellationToken)
    {
        var specification = new TradingAccountsByClientSpecification(request.ClientId, includeClient: true);
        var tradingAccounts = await tradingAccountRepository.ListAsync(specification, cancellationToken);

        var tradingAccountDtos = tradingAccounts.Select(t => new TradingAccountDto(
            t.Id,
            t.AccountLogin,
            t.Currency,
            t.Balance,
            t.Leverage,
            t.Server,
            t.Equity,
            t.IsDemo,
            t.ClientId,
            t.Client != null ? $"{t.Client.FirstName} {t.Client.LastName}" : null,
            t.CreatedAt)).ToList();

        return Result.Success(tradingAccountDtos);
    }
}