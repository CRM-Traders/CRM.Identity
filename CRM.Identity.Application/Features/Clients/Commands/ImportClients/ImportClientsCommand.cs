using ClosedXML.Excel;
using CRM.Identity.Application.Common.Specifications.Clients;
using CRM.Identity.Domain.Entities.Affiliate;
using CRM.Identity.Domain.Entities.Clients;

namespace CRM.Identity.Application.Features.Clients.Commands.ImportClients;

public sealed record ImportClientsCommand(byte[] FileContent) : IRequest<ImportClientsResult>;

public sealed record ImportClientsResult(
    int SuccessCount,
    int FailureCount,
    List<string> Errors);

public sealed class ImportClientsCommandValidator : AbstractValidator<ImportClientsCommand>
{
    public ImportClientsCommandValidator()
    {
        RuleFor(x => x.FileContent)
            .NotEmpty()
            .WithMessage("File content is required.");
    }
}

public sealed class ImportClientsCommandHandler(
    IRepository<Client> clientRepository,
    IRepository<Affiliate> affiliateRepository,
    IUnitOfWork unitOfWork,
    IUserContext userContext) : IRequestHandler<ImportClientsCommand, ImportClientsResult>
{
    public async ValueTask<Result<ImportClientsResult>> Handle(ImportClientsCommand request, CancellationToken cancellationToken)
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
                    var firstName = row.Cell(1).GetString().Trim();
                    var lastName = row.Cell(2).GetString().Trim();
                    var email = row.Cell(3).GetString().Trim().ToLower();
                    var affiliateIdStr = row.Cell(4).GetString().Trim();
                    var telephone = row.Cell(5).GetString().Trim();
                    var country = row.Cell(6).GetString().Trim();
                    var language = row.Cell(7).GetString().Trim();
                    var dateOfBirthStr = row.Cell(8).GetString().Trim();
                    var source = row.Cell(9).GetString().Trim();

                    if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(affiliateIdStr))
                    {
                        errors.Add($"Row {row.RowNumber()}: First Name, Last Name, Email, and Affiliate ID are required");
                        failureCount++;
                        continue;
                    }

                    if (!Guid.TryParse(affiliateIdStr, out var affiliateId))
                    {
                        errors.Add($"Row {row.RowNumber()}: Invalid Affiliate ID format");
                        failureCount++;
                        continue;
                    }

                    var affiliate = await affiliateRepository.GetByIdAsync(affiliateId, cancellationToken);
                    if (affiliate == null)
                    {
                        errors.Add($"Row {row.RowNumber()}: Affiliate not found");
                        failureCount++;
                        continue;
                    }

                    var emailSpecification = new ClientByEmailSpecification(email);
                    var existingClient = await clientRepository.FirstOrDefaultAsync(emailSpecification, cancellationToken);

                    if (existingClient != null)
                    {
                        errors.Add($"Row {row.RowNumber()}: Client with email {email} already exists");
                        failureCount++;
                        continue;
                    }

                    DateTime? dateOfBirth = null;
                    if (!string.IsNullOrEmpty(dateOfBirthStr) && DateTime.TryParse(dateOfBirthStr, out var parsedDate))
                    {
                        dateOfBirth = parsedDate;
                    }

                    var client = new Client(
                        firstName,
                        lastName,
                        email,
                        affiliateId,
                        string.IsNullOrEmpty(telephone) ? null : telephone,
                        string.IsNullOrEmpty(country) ? null : country,
                        string.IsNullOrEmpty(language) ? null : language,
                        dateOfBirth,
                        userContext.IpAddress,
                        "CRM System",
                        "Excel Import",
                        string.IsNullOrEmpty(source) ? null : source);

                    await clientRepository.AddAsync(client, cancellationToken);
                    successCount++;
                }
                catch (Exception ex)
                {
                    errors.Add($"Row {row.RowNumber()}: {ex.Message}");
                    failureCount++;
                }
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(new ImportClientsResult(successCount, failureCount, errors));
        }
        catch (Exception ex)
        {
            return Result.Failure<ImportClientsResult>($"Error processing file: {ex.Message}");
        }
    }
}