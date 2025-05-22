using ClosedXML.Excel;
using CRM.Identity.Application.Common.Specifications.Affiliates;
using CRM.Identity.Domain.Entities.Affiliate;

namespace CRM.Identity.Application.Features.Affiliates.Commands.ImportAffiliates;

public sealed record ImportAffiliatesCommand(byte[] FileContent) : IRequest<ImportAffiliatesResult>;

public sealed record ImportAffiliatesResult(
    int SuccessCount,
    int FailureCount,
    List<string> Errors);

public sealed class ImportAffiliatesCommandValidator : AbstractValidator<ImportAffiliatesCommand>
{
    public ImportAffiliatesCommandValidator()
    {
        RuleFor(x => x.FileContent)
            .NotEmpty()
            .WithMessage("File content is required.");
    }
}

public sealed class ImportAffiliatesCommandHandler(
    IRepository<Affiliate> affiliateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ImportAffiliatesCommand, ImportAffiliatesResult>
{
    public async ValueTask<Result<ImportAffiliatesResult>> Handle(ImportAffiliatesCommand request, CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        var successCount = 0;
        var failureCount = 0;

        try
        {
            using var stream = new MemoryStream(request.FileContent);
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);

            var rows = worksheet.RangeUsed()!.RowsUsed().Skip(1); // Skip header

            foreach (var row in rows)
            {
                try
                {
                    var name = row.Cell(1).GetString().Trim();
                    var email = row.Cell(2).GetString().Trim().ToLower();
                    var phone = row.Cell(3).GetString().Trim();
                    var website = row.Cell(4).GetString().Trim();

                    if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email))
                    {
                        errors.Add($"Row {row.RowNumber()}: Name and Email are required");
                        failureCount++;
                        continue;
                    }

                    var emailSpecification = new AffiliateByEmailSpecification(email);
                    var existingAffiliate = await affiliateRepository.FirstOrDefaultAsync(emailSpecification, cancellationToken);

                    if (existingAffiliate != null)
                    {
                        errors.Add($"Row {row.RowNumber()}: Affiliate with email {email} already exists");
                        failureCount++;
                        continue;
                    }

                    var affiliate = new Affiliate(
                        name,
                        email,
                        string.IsNullOrEmpty(phone) ? null : phone,
                        string.IsNullOrEmpty(website) ? null : website);

                    await affiliateRepository.AddAsync(affiliate, cancellationToken);
                    successCount++;
                }
                catch (Exception ex)
                {
                    errors.Add($"Row {row.RowNumber()}: {ex.Message}");
                    failureCount++;
                }
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(new ImportAffiliatesResult(successCount, failureCount, errors));
        }
        catch (Exception ex)
        {
            return Result.Failure<ImportAffiliatesResult>($"Error processing file: {ex.Message}");
        }
    }
}