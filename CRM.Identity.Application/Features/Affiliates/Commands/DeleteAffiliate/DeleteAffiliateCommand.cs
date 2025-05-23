using CRM.Identity.Application.Common.Specifications.Clients;
using CRM.Identity.Domain.Entities.Affiliate;
using CRM.Identity.Domain.Entities.Clients;

namespace CRM.Identity.Application.Features.Affiliates.Commands.DeleteAffiliate;

public sealed record DeleteAffiliateCommand(Guid Id) : IRequest<Unit>;

public sealed class DeleteAffiliateCommandValidator : AbstractValidator<DeleteAffiliateCommand>
{
    public DeleteAffiliateCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Affiliate ID is required.");
    }
}

public sealed class DeleteAffiliateCommandHandler(
    IRepository<Affiliate> affiliateRepository,
    IRepository<Client> clientRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteAffiliateCommand, Unit>
{
    public async ValueTask<Result<Unit>> Handle(DeleteAffiliateCommand request, CancellationToken cancellationToken)
    {
        var affiliate = await affiliateRepository.GetByIdAsync(request.Id, cancellationToken);
        if (affiliate == null)
        {
            return Result.Failure<Unit>("Affiliate not found", "NotFound");
        }

        var clientsSpecification = new ClientsByAffiliateSpecification(request.Id);
        var hasClients = await clientRepository.AnyAsync(clientsSpecification, cancellationToken);

        if (hasClients)
        {
            return Result.Failure<Unit>("Cannot delete affiliate with associated clients", "Conflict");
        }

        await affiliateRepository.DeleteAsync(affiliate, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Unit.Value);
    }
}