using ClosedXML.Excel;
using CRM.Identity.Application.Common.Specifications.Affiliates;
using CRM.Identity.Domain.Entities.Affiliate;

namespace CRM.Identity.Application.Features.Affiliates.Queries.ExportAffiliates;

public sealed record ExportAffiliatesQuery(
    string? SearchTerm,
    bool? IsActive) : IRequest<byte[]>;

public sealed class ExportAffiliatesQueryHandler(
    IRepository<Affiliate> affiliateRepository) : IRequestHandler<ExportAffiliatesQuery, byte[]>
{
    public async ValueTask<Result<byte[]>> Handle(ExportAffiliatesQuery request, CancellationToken cancellationToken)
    {
        var specification = new AffiliatesFilterSpecification(request.SearchTerm, request.IsActive);
        var affiliates = await affiliateRepository.ListAsync(specification, cancellationToken);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Affiliates");

        // Headers
        worksheet.Cell(1, 1).Value = "ID";
        worksheet.Cell(1, 2).Value = "Name";
        worksheet.Cell(1, 3).Value = "Email";
        worksheet.Cell(1, 4).Value = "Phone";
        worksheet.Cell(1, 5).Value = "Website";
        worksheet.Cell(1, 6).Value = "Is Active";
        worksheet.Cell(1, 7).Value = "Created At";

        // Style headers
        var headerRange = worksheet.Range(1, 1, 1, 7);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

        // Data
        for (int i = 0; i < affiliates.Count; i++)
        {
            var affiliate = affiliates[i];
            var row = i + 2;

            worksheet.Cell(row, 1).Value = affiliate.Id.ToString();
            worksheet.Cell(row, 2).Value = $"{affiliate.User!.FirstName} {affiliate.User!.LastName}";
            worksheet.Cell(row, 3).Value = affiliate.User.Email;
            worksheet.Cell(row, 4).Value = affiliate.Phone ?? "";
            worksheet.Cell(row, 5).Value = affiliate.Website ?? "";
            worksheet.Cell(row, 6).Value = affiliate.IsActive ? "Yes" : "No";
            worksheet.Cell(row, 7).Value = affiliate.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
        }

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Result.Success(stream.ToArray());
    }
}