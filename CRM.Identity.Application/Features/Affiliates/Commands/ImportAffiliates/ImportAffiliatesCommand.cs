using System.Security.Cryptography;
using System.Text;
using ClosedXML.Excel;
using CRM.Identity.Domain.Entities.Affiliate;

namespace CRM.Identity.Application.Features.Affiliates.Commands.ImportAffiliates;

public sealed record ImportAffiliatesCommand(byte[] FileContent) : IRequest<ImportAffiliatesResult>;

public sealed record ImportAffiliatesResult(
    int SuccessCount,
    int FailureCount,
    List<ImportAffiliateResult> AffiliateResults,
    List<string> Errors);

public sealed record ImportAffiliateResult(
    string Name,
    string Email,
    string GeneratedPassword,
    Guid AffiliateId,
    Guid UserId);

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
    IRepository<User> userRepository,
    IPasswordService passwordService,
    IUnitOfWork unitOfWork) : IRequestHandler<ImportAffiliatesCommand, ImportAffiliatesResult>
{
    public async ValueTask<Result<ImportAffiliatesResult>> Handle(ImportAffiliatesCommand request,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        var affiliateResults = new List<ImportAffiliateResult>();
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
                    var firstName = row.Cell(3).GetString().Trim();
                    var lastName = row.Cell(4).GetString().Trim();
                    var phone = row.Cell(5).GetString().Trim();
                    var website = row.Cell(6).GetString().Trim();

                    if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) ||
                        string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
                    {
                        errors.Add($"Row {row.RowNumber()}: Name, Email, First Name, and Last Name are required");
                        failureCount++;
                        continue;
                    }


                    var existingUser = await userRepository.FirstOrDefaultAsync(
                        new UserByEmailOrUsernameSpec(email, email), cancellationToken);

                    if (existingUser != null)
                    {
                        errors.Add($"Row {row.RowNumber()}: User with email {email} already exists");
                        failureCount++;
                        continue;
                    }

                    var generatedPassword = passwordService.GenerateStrongPassword();
                    var hashedPassword = passwordService.HashPasword(generatedPassword, out var salt);
                    var saltString = Convert.ToBase64String(salt);

                    var user = new User(firstName, lastName, email, email,
                        string.IsNullOrEmpty(phone) ? null : phone, hashedPassword, saltString);

                    var affiliate = new Affiliate(
                        string.IsNullOrEmpty(phone) ? null : phone,
                        string.IsNullOrEmpty(website) ? null : website)
                    {
                        UserId = user.Id
                    };

                    await userRepository.AddAsync(user, cancellationToken);
                    await affiliateRepository.AddAsync(affiliate, cancellationToken);

                    affiliateResults.Add(new ImportAffiliateResult(name, email, generatedPassword, affiliate.Id,
                        user.Id));
                    successCount++;
                }
                catch (Exception ex)
                {
                    errors.Add($"Row {row.RowNumber()}: {ex.Message}");
                    failureCount++;
                }
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(new ImportAffiliatesResult(successCount, failureCount, affiliateResults, errors));
        }
        catch (Exception ex)
        {
            return Result.Failure<ImportAffiliatesResult>($"Error processing file: {ex.Message}");
        }
    }
}