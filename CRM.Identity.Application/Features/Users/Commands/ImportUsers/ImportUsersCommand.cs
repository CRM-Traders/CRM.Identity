using System.Security.Cryptography;
using System.Text;
using ClosedXML.Excel;
using CRM.Identity.Domain.Entities.Users.Enums;

namespace CRM.Identity.Application.Features.Users.Commands.ImportUsers;

public sealed record ImportUsersCommand(byte[] FileContent) : IRequest<ImportUsersResult>;

public sealed record ImportUsersResult(
    int SuccessCount,
    int FailureCount,
    List<ImportUserResult> UserResults,
    List<string> Errors);

public sealed record ImportUserResult(
    string Email,
    string Username,
    string GeneratedPassword,
    string Role);

public sealed class ImportUsersCommandValidator : AbstractValidator<ImportUsersCommand>
{
    public ImportUsersCommandValidator()
    {
        RuleFor(x => x.FileContent)
            .NotEmpty()
            .WithMessage("File content is required.");
    }
}

public sealed class ImportUsersCommandHandler(
    IRepository<User> userRepository,
    IPasswordService passwordService,
    IUnitOfWork unitOfWork) : IRequestHandler<ImportUsersCommand, ImportUsersResult>
{
    public async ValueTask<Result<ImportUsersResult>> Handle(ImportUsersCommand request,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        var userResults = new List<ImportUserResult>();
        var successCount = 0;
        var failureCount = 0;

        try
        {
            using var stream = new MemoryStream(request.FileContent);
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);

            var rows = worksheet.RangeUsed()!.RowsUsed().Skip(1);

            foreach (var row in rows)
            {
                try
                {
                    var email = row.Cell(1).GetString().Trim().ToLower();
                    var username = row.Cell(2).GetString().Trim();
                    var firstName = row.Cell(3).GetString().Trim();
                    var lastName = row.Cell(4).GetString().Trim();
                    var roleStr = row.Cell(5).GetString().Trim();
                    var phoneNumber = row.Cell(6).GetString().Trim();

                    if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(username) ||
                        string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) ||
                        string.IsNullOrEmpty(roleStr))
                    {
                        errors.Add($"Row {row.RowNumber()}: All required fields must be filled");
                        failureCount++;
                        continue;
                    }

                    if (!Enum.TryParse<Role>(roleStr, true, out var role))
                    {
                        errors.Add($"Row {row.RowNumber()}: Invalid role. Valid values: Admin, SuperUser, User");
                        failureCount++;
                        continue;
                    }

                    var existingUser = await userRepository
                        .FirstOrDefaultAsync(new UserByEmailOrUsernameSpec(email, username), cancellationToken);

                    if (existingUser != null)
                    {
                        errors.Add($"Row {row.RowNumber()}: User with this email or username already exists");
                        failureCount++;
                        continue;
                    }

                    var generatedPassword = passwordService.GenerateStrongPassword();
                    var hashedPassword = passwordService.HashPasword(generatedPassword, out var salt);
                    var saltString = Convert.ToBase64String(salt);

                    var user = new User(firstName, lastName, email, username,
                        string.IsNullOrEmpty(phoneNumber) ? null : phoneNumber,
                        hashedPassword, saltString);

                    await userRepository.AddAsync(user, cancellationToken);
                    userResults.Add(new ImportUserResult(email, username, generatedPassword, role.ToString()));
                    successCount++;
                }
                catch (Exception ex)
                {
                    errors.Add($"Row {row.RowNumber()}: {ex.Message}");
                    failureCount++;
                }
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(new ImportUsersResult(successCount, failureCount, userResults, errors));
        }
        catch (Exception ex)
        {
            return Result.Failure<ImportUsersResult>($"Error processing file: {ex.Message}");
        }
    }
}