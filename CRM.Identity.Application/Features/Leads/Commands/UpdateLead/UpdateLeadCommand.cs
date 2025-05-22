using CRM.Identity.Application.Common.Specifications.Clients;
using CRM.Identity.Application.Common.Specifications.Leads;
using CRM.Identity.Domain.Entities.Clients;
using CRM.Identity.Domain.Entities.Leads;

namespace CRM.Identity.Application.Features.Leads.Commands.UpdateLead;

public sealed record UpdateLeadCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? Telephone,
    string? SecondTelephone,
    string? Skype,
    string? Country,
    string? Language,
    DateTime? DateOfBirth) : IRequest<Unit>;

public sealed class UpdateLeadCommandValidator : AbstractValidator<UpdateLeadCommand>
{
    public UpdateLeadCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Lead ID is required.");

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

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.Today)
            .When(x => x.DateOfBirth.HasValue)
            .WithMessage("Date of birth must be in the past.");
    }
}

public sealed class UpdateLeadCommandHandler(
    IRepository<Lead> leadRepository,
    IRepository<Client> clientRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateLeadCommand, Unit>
{
    public async ValueTask<Result<Unit>> Handle(UpdateLeadCommand request, CancellationToken cancellationToken)
    {
        var lead = await leadRepository.GetByIdAsync(request.Id, cancellationToken);
        if (lead == null)
        {
            return Result.Failure<Unit>("Lead not found", "NotFound");
        }

        var email = request.Email.Trim().ToLower();

        // Check if email is used by another lead
        var leadEmailSpecification = new LeadByEmailExcludingIdSpecification(email, request.Id);
        var existingLead = await leadRepository.FirstOrDefaultAsync(leadEmailSpecification, cancellationToken);

        if (existingLead != null)
        {
            return Result.Failure<Unit>("Lead with this email already exists", "Conflict");
        }

        // Check if email is used by a client
        var clientEmailSpecification = new ClientByEmailSpecification(email);
        var existingClient = await clientRepository.FirstOrDefaultAsync(clientEmailSpecification, cancellationToken);

        if (existingClient != null)
        {
            return Result.Failure<Unit>("Client with this email already exists", "Conflict");
        }

        lead.UpdateContactInformation(
            request.FirstName.Trim(),
            request.LastName.Trim(),
            email,
            request.Telephone?.Trim(),
            request.SecondTelephone?.Trim(),
            request.Skype?.Trim());

        await leadRepository.UpdateAsync(lead, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Unit.Value);
    }
}