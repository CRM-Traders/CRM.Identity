using CRM.Identity.Application.Common.Specifications.Clients;
using CRM.Identity.Application.Common.Specifications.Leads;
using CRM.Identity.Domain.Entities.Clients;
using CRM.Identity.Domain.Entities.Leads;

namespace CRM.Identity.Application.Features.Leads.Commands.CreateLead;

public sealed record CreateLeadCommand(
    string FirstName,
    string LastName,
    string Email,
    string? Telephone,
    string? Country,
    string? Language,
    DateTime? DateOfBirth,
    string? Source) : IRequest<CreateLeadResult>;

public sealed record CreateLeadResult(
    Guid LeadId,
    Guid UserId,
    string GeneratedPassword);

public sealed class CreateLeadCommandValidator : AbstractValidator<CreateLeadCommand>
{
    public CreateLeadCommandValidator()
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

public sealed class CreateLeadCommandHandler(
    IRepository<Lead> leadRepository,
    IRepository<Client> clientRepository,
    IRepository<User> userRepository,
    IPasswordService passwordService,
    IUnitOfWork unitOfWork,
    IUserContext userContext) : IRequestHandler<CreateLeadCommand, CreateLeadResult>
{
    public async ValueTask<Result<CreateLeadResult>> Handle(CreateLeadCommand request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLower();

        // Check if already exists as lead
        var leadEmailSpecification = new LeadByEmailSpecification(email);
        var existingLead = await leadRepository.FirstOrDefaultAsync(leadEmailSpecification, cancellationToken);

        if (existingLead != null)
        {
            return Result.Failure<CreateLeadResult>("Lead with this email already exists", "Conflict");
        }

        // Check if already exists as client
        var clientEmailSpecification = new ClientByEmailSpecification(email);
        var existingClient = await clientRepository.FirstOrDefaultAsync(clientEmailSpecification, cancellationToken);

        if (existingClient != null)
        {
            return Result.Failure<CreateLeadResult>("Client with this email already exists", "Conflict");
        }

        var existingUser = await userRepository.FirstOrDefaultAsync(
            new UserByEmailOrUsernameSpec(email, email), cancellationToken);

        if (existingUser != null)
        {
            return Result.Failure<CreateLeadResult>("User with this email already exists", "Conflict");
        }

        var generatedPassword = passwordService.GenerateStrongPassword();
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

        var lead = new Lead(
            request.FirstName.Trim(),
            request.LastName.Trim(),
            email,
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
        await leadRepository.AddAsync(lead, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateLeadResult(lead.Id, user.Id, generatedPassword));
    }
}