using System.Security.Cryptography;
using System.Text;
using CRM.Identity.Application.Common.Specifications.Clients;
using CRM.Identity.Application.Common.Specifications.Leads;
using CRM.Identity.Domain.Entities.Affiliate;
using CRM.Identity.Domain.Entities.Clients;
using CRM.Identity.Domain.Entities.Leads;

namespace CRM.Identity.Application.Features.Clients.Commands.CreateClient;

public sealed record CreateClientCommand(
    string FirstName,
    string LastName,
    string Email,
    Guid AffiliateId,
    string? Telephone,
    string? Country,
    string? Language,
    DateTime? DateOfBirth,
    string? Source) : IRequest<CreateClientResult>;

public sealed record CreateClientResult(
    Guid ClientId,
    Guid UserId,
    string GeneratedPassword);

public sealed class CreateClientCommandValidator : AbstractValidator<CreateClientCommand>
{
    public CreateClientCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("First name is required and cannot exceed 50 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("Last name is required and cannot exceed 50 characters.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(200)
            .WithMessage("A valid email address is required.");

        RuleFor(x => x.AffiliateId)
            .NotEmpty()
            .WithMessage("Affiliate ID is required.");

        RuleFor(x => x.Telephone)
            .MaximumLength(20)
            .Matches(@"^[+]?[\d\s()-]+$")
            .When(x => !string.IsNullOrEmpty(x.Telephone))
            .WithMessage("Phone number format is invalid.");

        RuleFor(x => x.Country)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.Country))
            .WithMessage("Country cannot exceed 100 characters.");

        RuleFor(x => x.Language)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.Language))
            .WithMessage("Language cannot exceed 50 characters.");

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.Today)
            .When(x => x.DateOfBirth.HasValue)
            .WithMessage("Date of birth must be in the past.");
    }
}

public sealed class CreateClientCommandHandler(
    IRepository<Client> clientRepository,
    IRepository<Lead> leadRepository,
    IRepository<Affiliate> affiliateRepository,
    IRepository<User> userRepository,
    IPasswordService passwordService,
    IUnitOfWork unitOfWork,
    IUserContext userContext) : IRequestHandler<CreateClientCommand, CreateClientResult>
{
    public async ValueTask<Result<CreateClientResult>> Handle(CreateClientCommand request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLower();

        // Check if affiliate exists
        var affiliate = await affiliateRepository.GetByIdAsync(request.AffiliateId, cancellationToken);
        if (affiliate == null)
        {
            return Result.Failure<CreateClientResult>("Affiliate not found", "NotFound");
        }

        // Check if already exists as client
        var clientEmailSpecification = new ClientByEmailSpecification(email);
        var existingClient = await clientRepository.FirstOrDefaultAsync(clientEmailSpecification, cancellationToken);

        if (existingClient != null)
        {
            return Result.Failure<CreateClientResult>("Client with this email already exists", "Conflict");
        }

        // Check if already exists as lead
        var leadEmailSpecification = new LeadByEmailSpecification(email);
        var existingLead = await leadRepository.FirstOrDefaultAsync(leadEmailSpecification, cancellationToken);

        if (existingLead != null)
        {
            return Result.Failure<CreateClientResult>("Lead with this email already exists", "Conflict");
        }

        // Check if user exists
        var existingUser = await userRepository.FirstOrDefaultAsync(
            new UserByEmailOrUsernameSpec(email, email), cancellationToken);

        if (existingUser != null)
        {
            return Result.Failure<CreateClientResult>("User with this email already exists", "Conflict");
        }

        var generatedPassword = GenerateStrongPassword();
        var hashedPassword = passwordService.HashPasword(generatedPassword, out var salt);
        var saltString = Convert.ToBase64String(salt);

        var user = new User(
            request.FirstName.Trim(),
            request.LastName.Trim(),
            email,
            email,
            request.Telephone?.Trim(),
            hashedPassword,
            saltString);

        var client = new Client(
            request.FirstName.Trim(),
            request.LastName.Trim(),
            email,
            request.AffiliateId,
            request.Telephone?.Trim(),
            request.Country?.Trim(),
            request.Language?.Trim(),
            request.DateOfBirth,
            userContext.IpAddress,
            "CRM System",
            "Web",
            request.Source?.Trim())
        {
            UserId = user.Id
        };

        await userRepository.AddAsync(user, cancellationToken);
        await clientRepository.AddAsync(client, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateClientResult(client.Id, user.Id, generatedPassword));
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