using CRM.Identity.Application.Common.Specifications.TradingAccounts;
using CRM.Identity.Domain.Entities.TradingAccounts;
using CRM.Identity.Domain.Entities.TradingAccounts.Enums;

namespace CRM.Identity.Application.Features.TradingAccounts.Queries.GetTradingAccounts;

public sealed record GetTradingAccountsQuery(
    Guid? ClientId,
    Currency? Currency,
    bool? IsDemo,
    string? SearchTerm,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<GetTradingAccountsQueryResponse>;

public sealed record GetTradingAccountsQueryResponse(
    List<TradingAccountDto> TradingAccounts,
    int TotalCount,
    int PageNumber,
    int PageSize);

public sealed record TradingAccountDto(
    Guid Id,
    string AccountLogin,
    Currency Currency,
    decimal Balance,
    string? Leverage,
    string? Server,
    decimal Equity,
    bool IsDemo,
    Guid ClientId,
    string? ClientName,
    DateTimeOffset CreatedAt);

public sealed class GetTradingAccountsQueryHandler(
    IRepository<TradingAccount> tradingAccountRepository)
    : IRequestHandler<GetTradingAccountsQuery, GetTradingAccountsQueryResponse>
{
    public async ValueTask<Result<GetTradingAccountsQueryResponse>> Handle(GetTradingAccountsQuery request,
        CancellationToken cancellationToken)
    {
        var specification = new TradingAccountsFilterSpecification(
            request.ClientId,
            request.Currency,
            request.IsDemo,
            request.SearchTerm,
            request.PageNumber,
            request.PageSize,
            includeClient: true);

        var tradingAccounts = await tradingAccountRepository.ListAsync(specification, cancellationToken);
        var totalCount = await tradingAccountRepository.CountAsync(new TradingAccountsCountSpecification(
            request.ClientId,
            request.Currency,
            request.IsDemo,
            request.SearchTerm), cancellationToken);

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

        var response = new GetTradingAccountsQueryResponse(
            tradingAccountDtos,
            totalCount,
            request.PageNumber,
            request.PageSize);

        return Result.Success(response);
    }
}