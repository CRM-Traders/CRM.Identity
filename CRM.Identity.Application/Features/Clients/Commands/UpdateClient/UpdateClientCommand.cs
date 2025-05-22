using CRM.Identity.Application.Common.Specifications.Clients;
using CRM.Identity.Domain.Entities.Clients;

namespace CRM.Identity.Application.Features.Clients.Commands.UpdateClient;

public sealed record UpdateClientCommand(
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

public sealed class UpdateClientCommandValidator : AbstractValidator<UpdateClientCommand>
{
    public UpdateClientCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Client ID is required.");

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

public sealed class UpdateClientCommandHandler(
    IRepository<Client> clientRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateClientCommand, Unit>
{
    public async ValueTask<Result<Unit>> Handle(UpdateClientCommand request, CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdAsync(request.Id, cancellationToken);
        if (client == null)
        {
            return Result.Failure<Unit>("Client not found", "NotFound");
        }

        var emailSpecification = new ClientByEmailExcludingIdSpecification(request.Email.Trim().ToLower(), request.Id);
        var existingClient = await clientRepository.FirstOrDefaultAsync(emailSpecification, cancellationToken);

        if (existingClient != null)
        {
            return Result.Failure<Unit>("Client with this email already exists", "Conflict");
        }

        client.UpdateContactInformation(
            request.FirstName.Trim(),
            request.LastName.Trim(),
            request.Email.Trim().ToLower(),
            request.Telephone?.Trim(),
            request.SecondTelephone?.Trim(),
            request.Skype?.Trim());

        await clientRepository.UpdateAsync(client, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Unit.Value);
    }
}