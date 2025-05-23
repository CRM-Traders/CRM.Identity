using CRM.Identity.Application.Common.Specifications.Clients;
using CRM.Identity.Domain.Entities.Affiliate;
using CRM.Identity.Domain.Entities.Clients;
using CRM.Identity.Domain.Entities.Leads;

namespace CRM.Identity.Application.Features.Leads.Commands.ConvertLeadToClient;

public sealed record ConvertLeadToClientCommand(
    Guid LeadId,
    Guid AffiliateId) : IRequest<Guid>;

public sealed class ConvertLeadToClientCommandValidator : AbstractValidator<ConvertLeadToClientCommand>
{
    public ConvertLeadToClientCommandValidator()
    {
        RuleFor(x => x.LeadId)
            .NotEmpty()
            .WithMessage("Lead ID is required.");

        RuleFor(x => x.AffiliateId)
            .NotEmpty()
            .WithMessage("Affiliate ID is required.");
    }
}

public sealed class ConvertLeadToClientCommandHandler(
    IRepository<Lead> leadRepository,
    IRepository<Client> clientRepository,
    IRepository<Affiliate> affiliateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ConvertLeadToClientCommand, Guid>
{
    public async ValueTask<Result<Guid>> Handle(ConvertLeadToClientCommand request, CancellationToken cancellationToken)
    {
        var lead = await leadRepository.GetByIdAsync(request.LeadId, cancellationToken);
        if (lead == null)
        {
            return Result.Failure<Guid>("Lead not found", "NotFound");
        }

        var affiliate = await affiliateRepository.GetByIdAsync(request.AffiliateId, cancellationToken);
        if (affiliate == null)
        {
            return Result.Failure<Guid>("Affiliate not found", "NotFound");
        }

        // Check if client with this email already exists
        var clientEmailSpecification = new ClientByEmailSpecification(lead.Email);
        var existingClient = await clientRepository.FirstOrDefaultAsync(clientEmailSpecification, cancellationToken);

        if (existingClient != null)
        {
            return Result.Failure<Guid>("Client with this email already exists", "Conflict");
        }

        var client = lead.ConvertToClient(request.AffiliateId);

        await clientRepository.AddAsync(client, cancellationToken);
        await leadRepository.DeleteAsync(lead, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(client.Id);
    }
}
