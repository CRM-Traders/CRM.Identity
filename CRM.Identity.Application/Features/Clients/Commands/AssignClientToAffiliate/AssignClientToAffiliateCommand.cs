using CRM.Identity.Domain.Entities.Affiliate;
using CRM.Identity.Domain.Entities.Clients;

namespace CRM.Identity.Application.Features.Clients.Commands.AssignClientToAffiliate;

public sealed record AssignClientToAffiliateCommand(
    Guid ClientId,
    Guid AffiliateId) : IRequest<Unit>;

public sealed class AssignClientToAffiliateCommandValidator : AbstractValidator<AssignClientToAffiliateCommand>
{
    public AssignClientToAffiliateCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("Client ID is required.");

        RuleFor(x => x.AffiliateId)
            .NotEmpty()
            .WithMessage("Affiliate ID is required.");
    }
}

public sealed class AssignClientToAffiliateCommandHandler(
    IRepository<Client> clientRepository,
    IRepository<Affiliate> affiliateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<AssignClientToAffiliateCommand, Unit>
{
    public async ValueTask<Result<Unit>> Handle(AssignClientToAffiliateCommand request, CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdAsync(request.ClientId, cancellationToken);
        if (client == null)
        {
            return Result.Failure<Unit>("Client not found", "NotFound");
        }

        var affiliate = await affiliateRepository.GetByIdAsync(request.AffiliateId, cancellationToken);
        if (affiliate == null)
        {
            return Result.Failure<Unit>("Affiliate not found", "NotFound");
        }

        client.AssignToAffiliate(request.AffiliateId);

        await clientRepository.UpdateAsync(client, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Unit.Value);
    }
}