using System.Security.Cryptography;
using System.Text;
using CRM.Identity.Domain.Entities.Affiliate;

namespace CRM.Identity.Application.Features.Affiliates.Commands.CreateAffiliate;

public sealed record CreateAffiliateCommand(
    string Name,
    string Email,
    string FirstName,
    string LastName,
    string? Phone,
    string? Website) : IRequest<CreateAffiliateResult>;

public sealed record CreateAffiliateResult(
    Guid AffiliateId,
    Guid UserId,
    string GeneratedPassword);

public sealed class CreateAffiliateCommandValidator : AbstractValidator<CreateAffiliateCommand>
{
    public CreateAffiliateCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Name is required and cannot exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(200)
            .WithMessage("A valid email address is required.");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("First name is required and cannot exceed 50 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("Last name is required and cannot exceed 50 characters.");

        RuleFor(x => x.Phone)
            .MaximumLength(20)
            .Matches(@"^[+]?[\d\s()-]+$")
            .When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Phone number format is invalid.");

        RuleFor(x => x.Website)
            .MaximumLength(200)
            .Matches(@"^https?://.+")
            .When(x => !string.IsNullOrEmpty(x.Website))
            .WithMessage("Website must be a valid URL.");
    }
}

public sealed class CreateAffiliateCommandHandler(
    IRepository<Affiliate> affiliateRepository,
    IRepository<User> userRepository,
    IPasswordService passwordService,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateAffiliateCommand, CreateAffiliateResult>
{
    public async ValueTask<Result<CreateAffiliateResult>> Handle(CreateAffiliateCommand request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLower();

        var existingUser = await userRepository.FirstOrDefaultAsync(
            new UserByEmailOrUsernameSpec(email, email), cancellationToken);

        if (existingUser != null)
        {
            return Result.Failure<CreateAffiliateResult>("User with this email already exists", "Conflict");
        }

        var generatedPassword = GenerateStrongPassword();
        var hashedPassword = passwordService.HashPasword(generatedPassword, out var salt);
        var saltString = Convert.ToBase64String(salt);

        var user = new User(
            request.FirstName.Trim(),
            request.LastName.Trim(),
            email, // username = email
            email,
            request.Phone?.Trim(),
            hashedPassword,
            saltString);

        var affiliate = new Affiliate(
            request.Phone?.Trim(),
            request.Website?.Trim())
        {
            UserId = user.Id
        };

        await userRepository.AddAsync(user, cancellationToken);
        await affiliateRepository.AddAsync(affiliate, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateAffiliateResult(affiliate.Id, user.Id, generatedPassword));
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