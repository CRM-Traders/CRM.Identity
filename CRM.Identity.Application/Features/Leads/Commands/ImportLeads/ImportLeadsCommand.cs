using ClosedXML.Excel;
using CRM.Identity.Application.Common.Specifications.Clients;
using CRM.Identity.Application.Common.Specifications.Leads;
using CRM.Identity.Domain.Entities.Clients;
using CRM.Identity.Domain.Entities.Leads;

namespace CRM.Identity.Application.Features.Leads.Commands.ImportLeads;

public sealed record ImportLeadsCommand(byte[] FileContent) : IRequest<ImportLeadsResult>;

public sealed record ImportLeadsResult(
    int SuccessCount,
    int FailureCount,
    List<string> Errors);

public sealed class ImportLeadsCommandValidator : AbstractValidator<ImportLeadsCommand>
{
    public ImportLeadsCommandValidator()
    {
        RuleFor(x => x.FileContent)
            .NotEmpty()
            .WithMessage("File content is required.");
    }
}

public sealed class ImportLeadsCommandHandler(
    IRepository<Lead> leadRepository,
    IRepository<Client> clientRepository,
    IUnitOfWork unitOfWork,
    IUserContext userContext) : IRequestHandler<ImportLeadsCommand, ImportLeadsResult>
{
    public async ValueTask<Result<ImportLeadsResult>> Handle(ImportLeadsCommand request,
        CancellationToken cancellationToken)
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
                    var telephone = row.Cell(4).GetString().Trim();
                    var country = row.Cell(5).GetString().Trim();
                    var language = row.Cell(6).GetString().Trim();
                    var dateOfBirthStr = row.Cell(7).GetString().Trim();
                    var source = row.Cell(8).GetString().Trim();

                    if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) ||
                        string.IsNullOrEmpty(email))
                    {
                        errors.Add($"Row {row.RowNumber()}: First Name, Last Name, and Email are required");
                        failureCount++;
                        continue;
                    }

                    // Check if already exists as lead
                    var leadEmailSpecification = new LeadByEmailSpecification(email);
                    var existingLead =
                        await leadRepository.FirstOrDefaultAsync(leadEmailSpecification, cancellationToken);

                    if (existingLead != null)
                    {
                        errors.Add($"Row {row.RowNumber()}: Lead with email {email} already exists");
                        failureCount++;
                        continue;
                    }

                    // Check if already exists as client
                    var clientEmailSpecification = new ClientByEmailSpecification(email);
                    var existingClient =
                        await clientRepository.FirstOrDefaultAsync(clientEmailSpecification, cancellationToken);

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

                    var lead = new Lead(
                        firstName,
                        lastName,
                        email,
                        string.IsNullOrEmpty(telephone) ? null : telephone,
                        string.IsNullOrEmpty(country) ? null : country,
                        string.IsNullOrEmpty(language) ? null : language,
                        dateOfBirth,
                        userContext.IpAddress,
                        "CRM System",
                        "Excel Import",
                        string.IsNullOrEmpty(source) ? null : source);

                    await leadRepository.AddAsync(lead, cancellationToken);
                    successCount++;
                }
                catch (Exception ex)
                {
                    errors.Add($"Row {row.RowNumber()}: {ex.Message}");
                    failureCount++;
                }
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(new ImportLeadsResult(successCount, failureCount, errors));
        }
        catch (Exception ex)
        {
            return Result.Failure<ImportLeadsResult>($"Error processing file: {ex.Message}");
        }
    }
}