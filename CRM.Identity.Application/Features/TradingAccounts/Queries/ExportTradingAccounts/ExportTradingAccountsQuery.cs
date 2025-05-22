using ClosedXML.Excel;
using CRM.Identity.Application.Common.Specifications.TradingAccounts;
using CRM.Identity.Domain.Entities.TradingAccounts;
using CRM.Identity.Domain.Entities.TradingAccounts.Enums;

namespace CRM.Identity.Application.Features.TradingAccounts.Queries.ExportTradingAccounts;

public sealed record ExportTradingAccountsQuery(
    Guid? ClientId,
    Currency? Currency,
    bool? IsDemo,
    string? SearchTerm) : IRequest<byte[]>;

public sealed class ExportTradingAccountsQueryHandler(
    IRepository<TradingAccount> tradingAccountRepository) : IRequestHandler<ExportTradingAccountsQuery, byte[]>
{
    public async ValueTask<Result<byte[]>> Handle(ExportTradingAccountsQuery request,
        CancellationToken cancellationToken)
    {
        var specification = new TradingAccountsFilterSpecification(
            request.ClientId,
            request.Currency,
            request.IsDemo,
            request.SearchTerm,
            includeClient: true);

        var tradingAccounts = await tradingAccountRepository.ListAsync(specification, cancellationToken);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("TradingAccounts");

        // Headers
        var headers = new[]
        {
            "ID", "Account Login", "Currency", "Balance", "Leverage", "Server",
            "Equity", "Is Demo", "Client ID", "Client Name", "Client Email", "Created At"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
        }

        // Style headers
        var headerRange = worksheet.Range(1, 1, 1, headers.Length);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

        // Data
        for (int i = 0; i < tradingAccounts.Count; i++)
        {
            var account = tradingAccounts[i];
            var row = i + 2;

            worksheet.Cell(row, 1).Value = account.Id.ToString();
            worksheet.Cell(row, 2).Value = account.AccountLogin;
            worksheet.Cell(row, 3).Value = account.Currency.ToString();
            worksheet.Cell(row, 4).Value = account.Balance;
            worksheet.Cell(row, 5).Value = account.Leverage ?? "";
            worksheet.Cell(row, 6).Value = account.Server ?? "";
            worksheet.Cell(row, 7).Value = account.Equity;
            worksheet.Cell(row, 8).Value = account.IsDemo ? "Yes" : "No";
            worksheet.Cell(row, 9).Value = account.ClientId.ToString();
            worksheet.Cell(row, 10).Value =
                account.Client != null ? $"{account.Client.FirstName} {account.Client.LastName}" : "";
            worksheet.Cell(row, 11).Value = account.Client?.Email ?? "";
            worksheet.Cell(row, 12).Value = account.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
        }

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Result.Success(stream.ToArray());
    }
}