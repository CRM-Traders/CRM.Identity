using ClosedXML.Excel;
using CRM.Identity.Application.Common.Specifications.Clients;
using CRM.Identity.Domain.Entities.Clients;
using CRM.Identity.Domain.Entities.Clients.Enums;

namespace CRM.Identity.Application.Features.Clients.Queries.ExportClients;

public sealed record ExportClientsQuery(
    string? SearchTerm,
    Guid? AffiliateId,
    ClientStatus? Status,
    bool? IsProblematic,
    bool? IsBonusAbuser) : IRequest<byte[]>;

public sealed class ExportClientsQueryHandler(
    IRepository<Client> clientRepository) : IRequestHandler<ExportClientsQuery, byte[]>
{
    public async ValueTask<Result<byte[]>> Handle(ExportClientsQuery request, CancellationToken cancellationToken)
    {
        var specification = new ClientsFilterSpecification(
            request.SearchTerm,
            request.AffiliateId,
            request.Status,
            request.IsProblematic,
            request.IsBonusAbuser,
            includeAffiliate: true);


        var clients = await clientRepository.ListAsync(specification, cancellationToken);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Clients");

        // Headers
        var headers = new[]
        {
            "ID", "First Name", "Last Name", "Email", "Telephone", "Country", "Language",
            "Date of Birth", "Status", "KYC Status", "Sales Status", "Is Problematic",
            "Is Bonus Abuser", "Bonus Abuser Reason", "Has Investments", "Affiliate ID",
            "Affiliate Name", "FTD Time", "LTD Time", "Qualification Time",
            "Registration Date", "Registration IP", "Source", "Last Login", "Last Communication"
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
        for (int i = 0; i < clients.Count; i++)
        {
            var client = clients[i];
            var row = i + 2;

            worksheet.Cell(row, 1).Value = client.Id.ToString();
            worksheet.Cell(row, 2).Value = client.FirstName;
            worksheet.Cell(row, 3).Value = client.LastName;
            worksheet.Cell(row, 4).Value = client.Email;
            worksheet.Cell(row, 5).Value = client.Telephone ?? "";
            worksheet.Cell(row, 6).Value = client.Country ?? "";
            worksheet.Cell(row, 7).Value = client.Language ?? "";
            worksheet.Cell(row, 8).Value = client.DateOfBirth?.ToString("yyyy-MM-dd") ?? "";
            worksheet.Cell(row, 9).Value = client.Status.ToString();
            worksheet.Cell(row, 10).Value = client.KycStatusId ?? "";
            worksheet.Cell(row, 11).Value = client.SalesStatus ?? "";
            worksheet.Cell(row, 12).Value = client.IsProblematic ? "Yes" : "No";
            worksheet.Cell(row, 13).Value = client.IsBonusAbuser ? "Yes" : "No";
            worksheet.Cell(row, 14).Value = client.BonusAbuserReason ?? "";
            worksheet.Cell(row, 15).Value = client.HasInvestments ? "Yes" : "No";
            worksheet.Cell(row, 16).Value = client.AffiliateId.ToString();
            worksheet.Cell(row, 17).Value = client.Affiliate?.Name ?? "";
            worksheet.Cell(row, 18).Value = client.FTDTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";
            worksheet.Cell(row, 19).Value = client.LTDTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";
            worksheet.Cell(row, 20).Value = client.QualificationTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";
            worksheet.Cell(row, 21).Value = client.RegistrationDate.ToString("yyyy-MM-dd HH:mm:ss");
            worksheet.Cell(row, 22).Value = client.RegistrationIP ?? "";
            worksheet.Cell(row, 23).Value = client.Source ?? "";
            worksheet.Cell(row, 24).Value = client.LastLogin?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";
            worksheet.Cell(row, 25).Value = client.LastCommunication?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";
        }

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Result.Success(stream.ToArray());
    }
}