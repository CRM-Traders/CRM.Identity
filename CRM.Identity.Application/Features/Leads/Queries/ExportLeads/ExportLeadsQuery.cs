using ClosedXML.Excel;
using CRM.Identity.Application.Common.Specifications.Leads;
using CRM.Identity.Domain.Entities.Clients.Enums;
using CRM.Identity.Domain.Entities.Leads;

namespace CRM.Identity.Application.Features.Leads.Queries.ExportLeads;

public sealed record ExportLeadsQuery(
    string? SearchTerm,
    ClientStatus? Status,
    bool? IsProblematic,
    string? Country,
    string? Source) : IRequest<byte[]>;

public sealed class ExportLeadsQueryHandler(
    IRepository<Lead> leadRepository) : IRequestHandler<ExportLeadsQuery, byte[]>
{
    public async ValueTask<Result<byte[]>> Handle(ExportLeadsQuery request, CancellationToken cancellationToken)
    {
        var specification = new LeadsFilterSpecification(
            request.SearchTerm,
            request.Status,
            request.IsProblematic,
            request.Country,
            request.Source,
            includeUser: true);

        var leads = await leadRepository.ListAsync(specification, cancellationToken);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Leads");

        // Headers
        var headers = new[]
        {
            "ID", "First Name", "Last Name", "Email", "Telephone", "Second Telephone", "Skype",
            "Country", "Language", "Date of Birth", "Status", "KYC Status", "Sales Status",
            "Is Problematic", "Is Bonus Abuser", "Bonus Abuser Reason", "Registration Date",
            "Registration IP", "Source", "Last Login", "Last Communication",
            "User ID", "Username", "User Email", "User Phone", "User Role"
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
        for (int i = 0; i < leads.Count; i++)
        {
            var lead = leads[i];
            var row = i + 2;

            worksheet.Cell(row, 1).Value = lead.Id.ToString();
            worksheet.Cell(row, 2).Value = lead.FirstName;
            worksheet.Cell(row, 3).Value = lead.LastName;
            worksheet.Cell(row, 4).Value = lead.Email;
            worksheet.Cell(row, 5).Value = lead.Telephone ?? "";
            worksheet.Cell(row, 6).Value = lead.SecondTelephone ?? "";
            worksheet.Cell(row, 7).Value = lead.Skype ?? "";
            worksheet.Cell(row, 8).Value = lead.Country ?? "";
            worksheet.Cell(row, 9).Value = lead.Language ?? "";
            worksheet.Cell(row, 10).Value = lead.DateOfBirth?.ToString("yyyy-MM-dd") ?? "";
            worksheet.Cell(row, 11).Value = lead.Status.ToString();
            worksheet.Cell(row, 12).Value = lead.KycStatusId ?? "";
            worksheet.Cell(row, 13).Value = lead.SalesStatus ?? "";
            worksheet.Cell(row, 14).Value = lead.IsProblematic ? "Yes" : "No";
            worksheet.Cell(row, 15).Value = lead.IsBonusAbuser ? "Yes" : "No";
            worksheet.Cell(row, 16).Value = lead.BonusAbuserReason ?? "";
            worksheet.Cell(row, 17).Value = lead.RegistrationDate.ToString("yyyy-MM-dd HH:mm:ss");
            worksheet.Cell(row, 18).Value = lead.RegistrationIP ?? "";
            worksheet.Cell(row, 19).Value = lead.Source ?? "";
            worksheet.Cell(row, 20).Value = lead.LastLogin?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";
            worksheet.Cell(row, 21).Value = lead.LastCommunication?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";

            // User information
            worksheet.Cell(row, 22).Value = lead.User?.Id.ToString() ?? "";
            worksheet.Cell(row, 23).Value = lead.User?.Username ?? "";
            worksheet.Cell(row, 24).Value = lead.User?.Email ?? "";
            worksheet.Cell(row, 25).Value = lead.User?.PhoneNumber ?? "";
            worksheet.Cell(row, 26).Value = lead.User?.Role.ToString() ?? "";
        }

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Result.Success(stream.ToArray());
    }
}