using CRM.Identity.Application.Features.TradingAccounts.Commands.CreateTradingAccount;
using CRM.Identity.Application.Features.TradingAccounts.Commands.DeleteTradingAccount;
using CRM.Identity.Application.Features.TradingAccounts.Commands.UpdateTradingAccount;
using CRM.Identity.Application.Features.TradingAccounts.Queries.ExportTradingAccounts;
using CRM.Identity.Application.Features.TradingAccounts.Queries.GetTradingAccountById;
using CRM.Identity.Application.Features.TradingAccounts.Queries.GetTradingAccounts;
using CRM.Identity.Application.Features.TradingAccounts.Queries.GetTradingAccountsByClient;
using CRM.Identity.Domain.Entities.TradingAccounts.Enums;
using CRM.Identity.Infrastructure.Attributes;

namespace CRM.Identity.Api.Controllers;

public class TradingAccountsController(IMediator mediator) : BaseController(mediator)
{
    [HttpPost]
    [Permission(50, "Create Trading Account", "TradingAccounts", ActionType.C, RoleConstants.AllExceptUser)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IResult> CreateTradingAccount([FromBody] CreateTradingAccountCommand command,
        CancellationToken cancellationToken)
    {
        return await SendAsync(command, cancellationToken);
    }

    [HttpPut("{id}")]
    [Permission(51, "Update Trading Account", "TradingAccounts", ActionType.E, RoleConstants.AllExceptUser)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> UpdateTradingAccount(Guid id, [FromBody] UpdateTradingAccountCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return Results.BadRequest("ID mismatch");

        return await SendAsync(command, cancellationToken);
    }

    [HttpDelete("{id}")]
    [Permission(52, "Delete Trading Account", "TradingAccounts", ActionType.D, RoleConstants.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> DeleteTradingAccount(Guid id, CancellationToken cancellationToken)
    {
        return await SendAsync(new DeleteTradingAccountCommand(id), cancellationToken);
    }

    [HttpGet]
    [Permission(53, "View Trading Accounts", "TradingAccounts", ActionType.V, RoleConstants.All)]
    [ProducesResponseType(typeof(GetTradingAccountsQueryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> GetTradingAccounts(
        [FromQuery] Guid? clientId,
        [FromQuery] Currency? currency,
        [FromQuery] bool? isDemo,
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        return await SendAsync(
            new GetTradingAccountsQuery(clientId, currency, isDemo, searchTerm, pageNumber, pageSize),
            cancellationToken);
    }

    [HttpGet("{id}")]
    [Permission(54, "View Trading Account Details", "TradingAccounts", ActionType.V, RoleConstants.All)]
    [ProducesResponseType(typeof(TradingAccountDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetTradingAccountById(Guid id, CancellationToken cancellationToken)
    {
        return await SendAsync(new GetTradingAccountByIdQuery(id), cancellationToken);
    }

    [HttpGet("by-client/{clientId}")]
    [Permission(55, "View Trading Accounts by Client", "TradingAccounts", ActionType.V, RoleConstants.All)]
    [ProducesResponseType(typeof(List<TradingAccountDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> GetTradingAccountsByClient(Guid clientId, CancellationToken cancellationToken)
    {
        return await SendAsync(new GetTradingAccountsByClientQuery(clientId), cancellationToken);
    }

    [HttpGet("export")]
    [Permission(56, "Export Trading Accounts", "TradingAccounts", ActionType.V, RoleConstants.AllExceptUser)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> ExportTradingAccounts(
        [FromQuery] Guid? clientId,
        [FromQuery] Currency? currency,
        [FromQuery] bool? isDemo,
        [FromQuery] string? searchTerm,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ExportTradingAccountsQuery(clientId, currency, isDemo, searchTerm),
            cancellationToken);

        if (!result.IsSuccess)
            return ToResult(result);

        var fileName = $"trading_accounts_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
        return Results.File(result.Value!, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    [HttpGet("by-client/{clientId}/export")]
    [Permission(57, "Export Trading Accounts by Client", "TradingAccounts", ActionType.V, RoleConstants.AllExceptUser)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> ExportTradingAccountsByClient(
        Guid clientId,
        [FromQuery] Currency? currency,
        [FromQuery] bool? isDemo,
        [FromQuery] string? searchTerm,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ExportTradingAccountsQuery(clientId, currency, isDemo, searchTerm),
            cancellationToken);

        if (!result.IsSuccess)
            return ToResult(result);

        var fileName = $"trading_accounts_client_{clientId}_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
        return Results.File(result.Value!, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    [HttpGet("summary")]
    [Permission(58, "View Trading Accounts Summary", "TradingAccounts", ActionType.V, RoleConstants.All)]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IResult> GetTradingAccountsSummary(
        [FromQuery] Guid? clientId,
        CancellationToken cancellationToken = default)
    {
        var accounts = await mediator.Send(new GetTradingAccountsQuery(clientId, null, null, null, 1, int.MaxValue),
            cancellationToken);

        if (!accounts.IsSuccess)
            return ToResult(accounts);

        var summary = new
        {
            TotalAccounts = accounts.Value!.TotalCount,
            DemoAccounts = accounts.Value.TradingAccounts.Count(a => a.IsDemo),
            LiveAccounts = accounts.Value.TradingAccounts.Count(a => !a.IsDemo),
            TotalBalance = accounts.Value.TradingAccounts.Sum(a => a.Balance),
            TotalEquity = accounts.Value.TradingAccounts.Sum(a => a.Equity),
            CurrencyBreakdown = accounts.Value.TradingAccounts
                .GroupBy(a => a.Currency)
                .Select(g => new
                    { Currency = g.Key.ToString(), Count = g.Count(), TotalBalance = g.Sum(a => a.Balance) })
                .ToList()
        };

        return Results.Ok(summary);
    }
}