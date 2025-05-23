using ClosedXML.Excel;

namespace CRM.Identity.Application.Features.Clients.Queries.GenerateClientTemplate;
public sealed record GenerateClientTemplateQuery : IRequest<byte[]>;

public sealed class GenerateClientTemplateQueryHandler : IRequestHandler<GenerateClientTemplateQuery, byte[]>
{
    public ValueTask<Result<byte[]>> Handle(GenerateClientTemplateQuery request, CancellationToken cancellationToken)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Clients");

        var headers = new[]
        {
            "First Name*",
            "Last Name*",
            "Email*",
            "Affiliate ID*",
            "Telephone",
            "Country",
            "Language",
            "Date of Birth (yyyy-MM-dd)",
            "Source"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#4CAF50");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        var sampleData = new[]
        {
            new[]
            {
                "John", "Smith", "john.smith@example.com", "00000000-0000-0000-0000-000000000001", "+1234567890", "United States", "English", "1990-01-15", "Website"
            },
            new[] { "Jane", "Doe", "jane.doe@example.com", "00000000-0000-0000-0000-000000000002", "+0987654321", "Canada", "English", "1985-05-20", "Social Media" },
            new[] { "Bob", "Wilson", "bob.wilson@example.com", "00000000-0000-0000-0000-000000000001", "", "United Kingdom", "English", "1992-12-10", "Referral" }
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

        // Add notes/instructions
        var notesStartRow = sampleData.Length + 4;
        worksheet.Cell(notesStartRow, 1).Value = "Instructions:";
        worksheet.Cell(notesStartRow, 1).Style.Font.Bold = true;
        worksheet.Cell(notesStartRow, 1).Style.Font.FontColor = XLColor.Red;

        var instructions = new[]
        {
            "• Fields marked with * are required",
            "• Email must be unique and valid format",
            "• Affiliate ID must be valid GUID format and exist in system",
            "• Date of Birth format: yyyy-MM-dd (e.g., 1990-01-15)",
            "• Telephone format: +[country code][number] (optional)",
            "• A user account will be automatically created for each client",
            "• Generated passwords will be provided after import"
        };

        for (int i = 0; i < instructions.Length; i++)
        {
            worksheet.Cell(notesStartRow + 1 + i, 1).Value = instructions[i];
            worksheet.Cell(notesStartRow + 1 + i, 1).Style.Font.FontSize = 10;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return new ValueTask<Result<byte[]>>(Result.Success(stream.ToArray()));
    }
}