using ClosedXML.Excel;

namespace CRM.Identity.Application.Features.Affiliates.Queries.GenerateAffiliateTemplate;

public sealed record GenerateAffiliateTemplateQuery : IRequest<byte[]>;

public sealed class GenerateAffiliateTemplateQueryHandler : IRequestHandler<GenerateAffiliateTemplateQuery, byte[]>
{
    public ValueTask<Result<byte[]>> Handle(GenerateAffiliateTemplateQuery request, CancellationToken cancellationToken)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Affiliates");

        var headers = new[]
        {
            "Name*",
            "Email*",
            "First Name*",
            "Last Name*",
            "Phone",
            "Website"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#5C946E");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        var sampleData = new[]
        {
            new[]
            {
                "Digital Marketing Pro", "contact@digitalmarketing.com", "John", "Smith", "+1234567890", "https://digitalmarketing.com"
            },
            new[] { "Social Media Experts", "info@socialmedia.com", "Jane", "Doe", "+0987654321", "https://socialmedia.com" },
            new[] { "Content Creators Hub", "hello@contentcreators.com", "Bob", "Wilson", "", "https://contentcreators.com" }
        };

        for (int row = 0; row < sampleData.Length; row++)
        {
            for (int col = 0; col < sampleData[row].Length; col++)
            {
                var cell = worksheet.Cell(row + 2, col + 1);
                cell.Value = sampleData[row][col];
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                if (row % 2 == 0)
                {
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#E8F5E9");
                }
            }
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return new ValueTask<Result<byte[]>>(Result.Success(stream.ToArray()));
    }
}
