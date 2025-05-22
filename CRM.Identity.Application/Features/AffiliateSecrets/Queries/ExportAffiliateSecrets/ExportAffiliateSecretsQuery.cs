using ClosedXML.Excel;
using CRM.Identity.Application.Common.Specifications.AffiliateSecrets;
using CRM.Identity.Domain.Entities.Affiliate;

namespace CRM.Identity.Application.Features.AffiliateSecrets.Queries.ExportAffiliateSecrets;

public sealed record ExportAffiliateSecretsQuery(
    Guid? AffiliateId,
    bool? IsActive,
    bool? IsExpired) : IRequest<byte[]>;

public sealed class ExportAffiliateSecretsQueryHandler(
    IRepository<AffiliateSecret> affiliateSecretRepository) : IRequestHandler<ExportAffiliateSecretsQuery, byte[]>
{
    public async ValueTask<Result<byte[]>> Handle(ExportAffiliateSecretsQuery request,
        CancellationToken cancellationToken)
    {
        var specification = new AffiliateSecretsFilterSpecification(
            request.AffiliateId,
            request.IsActive,
            request.IsExpired,
            includeAffiliate: true);

        var affiliateSecrets = await affiliateSecretRepository.ListAsync(specification, cancellationToken);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("AffiliateSecrets");

        // Headers
        var headers = new[]
        {
            "ID", "Affiliate ID", "Affiliate Name", "Secret Key", "API Key",
            "Expiration Date", "IP Restriction", "Is Active", "Used Count",
            "Is Expired", "Created At", "Last Modified At"
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
        for (int i = 0; i < affiliateSecrets.Count; i++)
        {
            var secret = affiliateSecrets[i];
            var row = i + 2;

            worksheet.Cell(row, 1).Value = secret.Id.ToString();
            worksheet.Cell(row, 2).Value = secret.AffiliateId.ToString();
            worksheet.Cell(row, 3).Value = secret.Affiliate?.Name ?? "";
            worksheet.Cell(row, 4).Value = secret.SecretKey;
            worksheet.Cell(row, 5).Value = secret.ApiKey;
            worksheet.Cell(row, 6).Value = secret.ExpirationDate.ToString("yyyy-MM-dd HH:mm:ss");
            worksheet.Cell(row, 7).Value = secret.IpRestriction ?? "";
            worksheet.Cell(row, 8).Value = secret.IsActive ? "Yes" : "No";
            worksheet.Cell(row, 9).Value = secret.UsedCount;
            worksheet.Cell(row, 10).Value = secret.IsExpired() ? "Yes" : "No";
            worksheet.Cell(row, 11).Value = secret.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
            worksheet.Cell(row, 12).Value = secret.LastModifiedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";
        }

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Result.Success(stream.ToArray());
    }
}