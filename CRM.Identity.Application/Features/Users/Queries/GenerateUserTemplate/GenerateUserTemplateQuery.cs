using ClosedXML.Excel;

namespace CRM.Identity.Application.Features.Users.Queries.GenerateUserTemplate;

public sealed record GenerateUserTemplateQuery : IRequest<byte[]>;

public sealed class GenerateUserTemplateQueryHandler : IRequestHandler<GenerateUserTemplateQuery, byte[]>
{
    public ValueTask<Result<byte[]>> Handle(GenerateUserTemplateQuery request, CancellationToken cancellationToken)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Users");

        var headers = new[]
        {
            "Email*",
            "Username*",
            "First Name*",
            "Last Name*",
            "Role* (Admin/SuperUser/User)",
            "Phone Number"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2E86AB");
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        var sampleData = new[]
        {
            new[] { "john.doe@example.com", "john.doe", "John", "Doe", "User", "+1234567890" },
            new[] { "jane.smith@example.com", "jane.smith", "Jane", "Smith", "Admin", "+0987654321" },
            new[] { "mike.johnson@example.com", "mike.j", "Mike", "Johnson", "SuperUser", "" }
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
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#F8F9FA");
                }
            }
        }

        worksheet.Columns().AdjustToContents();

        var roleRange = worksheet.Range(2, 5, 1000, 5);
        roleRange.CreateDataValidation()
            .List("Admin,SuperUser,User");

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return new ValueTask<Result<byte[]>>(Result.Success(stream.ToArray()));
    }
}