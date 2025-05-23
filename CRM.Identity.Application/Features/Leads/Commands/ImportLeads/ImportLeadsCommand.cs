using System.Security.Cryptography;
using System.Text;
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
    List<ImportLeadResult> LeadResults,
    List<string> Errors);

public sealed record ImportLeadResult(
    string FirstName,
    string LastName,
    string Email,
    string GeneratedPassword,
    Guid LeadId,
    Guid UserId);

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
    IRepository<User> userRepository,
    IPasswordService passwordService,
    IUnitOfWork unitOfWork,
    IUserContext userContext) : IRequestHandler<ImportLeadsCommand, ImportLeadsResult>
{
    public async ValueTask<Result<ImportLeadsResult>> Handle(ImportLeadsCommand request,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        var leadResults = new List<ImportLeadResult>();
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

                    // Check if user exists
                    var existingUser = await userRepository.FirstOrDefaultAsync(
                        new UserByEmailOrUsernameSpec(email, email), cancellationToken);

                    if (existingUser != null)
                    {
                        errors.Add($"Row {row.RowNumber()}: User with email {email} already exists");
                        failureCount++;
                        continue;
                    }

                    DateTime? dateOfBirth = null;
                    if (!string.IsNullOrEmpty(dateOfBirthStr) && DateTime.TryParse(dateOfBirthStr, out var parsedDate))
                    {
                        dateOfBirth = parsedDate;
                    }

                    var generatedPassword = GenerateStrongPassword();
                    var hashedPassword = passwordService.HashPasword(generatedPassword, out var salt);
                    var saltString = Convert.ToBase64String(salt);

                    var user = new User(firstName, lastName, email, email,
                        string.IsNullOrEmpty(telephone) ? null : telephone, hashedPassword, saltString);

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
                        string.IsNullOrEmpty(source) ? null : source)
                    {
                        UserId = user.Id
                    };

                    await userRepository.AddAsync(user, cancellationToken);
                    await leadRepository.AddAsync(lead, cancellationToken);
                    
                    leadResults.Add(new ImportLeadResult(firstName, lastName, email, generatedPassword, lead.Id, user.Id));
                    successCount++;
                }
                catch (Exception ex)
                {
                    errors.Add($"Row {row.RowNumber()}: {ex.Message}");
                    failureCount++;
                }
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(new ImportLeadsResult(successCount, failureCount, leadResults, errors));
        }
        catch (Exception ex)
        {
            return Result.Failure<ImportLeadsResult>($"Error processing file: {ex.Message}");
        }
    }

    private static string GenerateStrongPassword()
    {
        const string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lowerCase = "abcdefghijklmnopqrstuvwxyz";
        const string digits = "0123456789";
        const string specialChars = "!@#$%^&*()_-+=<>?";

        var password = new StringBuilder();
        using var rng = RandomNumberGenerator.Create();

        password.Append(GetRandomChar(upperCase, rng));
        password.Append(GetRandomChar(lowerCase, rng));
        password.Append(GetRandomChar(digits, rng));
        password.Append(GetRandomChar(specialChars, rng));

        var allChars = upperCase + lowerCase + digits + specialChars;
        for (int i = 0; i < 8; i++)
        {
            password.Append(GetRandomChar(allChars, rng));
        }

        return ShuffleString(password.ToString(), rng);
    }

    private static char GetRandomChar(string chars, RandomNumberGenerator rng)
    {
        var data = new byte[4];
        rng.GetBytes(data);
        var value = BitConverter.ToUInt32(data, 0);
        return chars[(int)(value % (uint)chars.Length)];
    }

    private static string ShuffleString(string input, RandomNumberGenerator rng)
    {
        var array = input.ToCharArray();
        var n = array.Length;
        while (n > 1)
        {
            var data = new byte[4];
            rng.GetBytes(data);
            var k = (int)(BitConverter.ToUInt32(data, 0) % (uint)n);
            n--;
            (array[n], array[k]) = (array[k], array[n]);
        }

        return new string(array);
    }
}