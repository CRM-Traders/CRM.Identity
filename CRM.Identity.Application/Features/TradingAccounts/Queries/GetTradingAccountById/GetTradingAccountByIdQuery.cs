using CRM.Identity.Application.Common.Specifications.TradingAccounts;
using CRM.Identity.Domain.Entities.TradingAccounts;
using CRM.Identity.Domain.Entities.TradingAccounts.Enums;

namespace CRM.Identity.Application.Features.TradingAccounts.Queries.GetTradingAccountById;

public sealed record GetTradingAccountByIdQuery(Guid Id) : IRequest<TradingAccountDetailDto>;

public sealed record TradingAccountDetailDto(
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
    string? ClientEmail,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastModifiedAt);

public sealed class GetTradingAccountByIdQueryHandler(
    IRepository<TradingAccount> tradingAccountRepository)
    : IRequestHandler<GetTradingAccountByIdQuery, TradingAccountDetailDto>
{
    public async ValueTask<Result<TradingAccountDetailDto>> Handle(GetTradingAccountByIdQuery request,
        CancellationToken cancellationToken)
    {
        var specification = new TradingAccountByIdWithClientSpecification(request.Id);
        var tradingAccount = await tradingAccountRepository.FirstOrDefaultAsync(specification, cancellationToken);

        if (tradingAccount == null)
        {
            return Result.Failure<TradingAccountDetailDto>("Trading account not found", "NotFound");
        }

        var response = new TradingAccountDetailDto(
            tradingAccount.Id,
            tradingAccount.AccountLogin,
            tradingAccount.Currency,
            tradingAccount.Balance,
            tradingAccount.Leverage,
            tradingAccount.Server,
            tradingAccount.Equity,
            tradingAccount.IsDemo,
            tradingAccount.ClientId,
            tradingAccount.Client != null
                ? $"{tradingAccount.Client.FirstName} {tradingAccount.Client.LastName}"
                : null,
            tradingAccount.Client?.Email,
            tradingAccount.CreatedAt,
            tradingAccount.LastModifiedAt);

        return Result.Success(response);
    }
}