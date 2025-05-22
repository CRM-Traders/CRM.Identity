using ClosedXML.Excel;

namespace CRM.Identity.Application.Features.Leads.Queries.GenerateLeadTemplate;

public sealed record GenerateLeadTemplateQuery : IRequest<byte[]>;

public sealed class GenerateLeadTemplateQueryHandler : IRequestHandler<GenerateLeadTemplateQuery, byte[]>
{
    public ValueTask<Result<byte[]>> Handle(GenerateLeadTemplateQuery request, CancellationToken cancellationToken)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Leads");

        var headers = new[]
        {
            "First Name*",
            "Last Name*",
            "Email*",
            "Telephone",
            "Country",
            "Language",
            "Date of Birth (YYYY-MM-DD)",
            "Source"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#4ECDC4");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        var sampleData = new[]
        {
            new[] { "Emma", "Johnson", "emma.johnson@email.com", "+1234567890", "USA", "en", "1988-03-25", "LinkedIn" },
            new[]
            {
                "Pierre", "Dubois", "pierre.dubois@email.com", "+33123456789", "France", "fr", "1995-11-30",
                "Google Search"
            },
            new[] { "Yuki", "Tanaka", "yuki.tanaka@email.com", "", "Japan", "ja", "1991-07-18", "Referral" }
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
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#E0F7F6");
                }
            }
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return new ValueTask<Result<byte[]>>(Result.Success(stream.ToArray()));
    }
}