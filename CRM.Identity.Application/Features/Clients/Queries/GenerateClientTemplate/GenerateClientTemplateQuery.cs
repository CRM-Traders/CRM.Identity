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
            "Date of Birth (YYYY-MM-DD)",
            "Source"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#FF6B6B");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        var sampleData = new[]
        {
            new[] { "John", "Smith", "john.smith@email.com", "550e8400-e29b-41d4-a716-446655440000", "+44123456789", "UK", "en", "1990-05-15", "Google Ads" },
            new[] { "Maria", "Garcia", "maria.garcia@email.com", "550e8400-e29b-41d4-a716-446655440000", "+34987654321", "Spain", "es", "1985-08-22", "Facebook" },
            new[] { "Li", "Wang", "li.wang@email.com", "550e8400-e29b-41d4-a716-446655440001", "", "China", "zh", "1992-01-10", "Organic" }
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
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#FFE5E5");
                }
            }
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return new ValueTask<Result<byte[]>>(Result.Success(stream.ToArray()));
    }
}