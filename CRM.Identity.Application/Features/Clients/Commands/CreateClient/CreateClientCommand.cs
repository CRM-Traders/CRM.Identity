using CRM.Identity.Application.Common.Specifications.Clients;
using CRM.Identity.Domain.Entities.Affiliate;
using CRM.Identity.Domain.Entities.Clients;

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
    string? Source) : IRequest<Guid>;

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
    IRepository<Affiliate> affiliateRepository,
    IUnitOfWork unitOfWork,
    IUserContext userContext) : IRequestHandler<CreateClientCommand, Guid>
{
    public async ValueTask<Result<Guid>> Handle(CreateClientCommand request, CancellationToken cancellationToken)
    {
        var affiliate = await affiliateRepository.GetByIdAsync(request.AffiliateId, cancellationToken);
        if (affiliate == null)
        {
            return Result.Failure<Guid>("Affiliate not found", "NotFound");
        }

        var emailSpecification = new ClientByEmailSpecification(request.Email.Trim().ToLower());
        var existingClient = await clientRepository.FirstOrDefaultAsync(emailSpecification, cancellationToken);

        if (existingClient != null)
        {
            return Result.Failure<Guid>("Client with this email already exists", "Conflict");
        }

        var client = new Client(
            request.FirstName.Trim(),
            request.LastName.Trim(),
            request.Email.Trim().ToLower(),
            request.AffiliateId,
            request.Telephone?.Trim(),
            request.Country?.Trim(),
            request.Language?.Trim(),
            request.DateOfBirth,
            userContext.IpAddress,
            "CRM System",
            "Web",
            request.Source?.Trim());

        await clientRepository.AddAsync(client, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(client.Id);
    }
}