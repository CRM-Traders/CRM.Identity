using CRM.Identity.Domain.Entities.Affiliate;

namespace CRM.Identity.Application.Features.AffiliateSecrets.Commands.DeleteAffiliateSecret;

public sealed record DeleteAffiliateSecretCommand(Guid Id) : IRequest<Unit>;

public sealed class DeleteAffiliateSecretCommandValidator : AbstractValidator<DeleteAffiliateSecretCommand>
{
    public DeleteAffiliateSecretCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Affiliate Secret ID is required.");
    }
}

public sealed class DeleteAffiliateSecretCommandHandler(
    IRepository<AffiliateSecret> affiliateSecretRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteAffiliateSecretCommand, Unit>
{
    public async ValueTask<Result<Unit>> Handle(DeleteAffiliateSecretCommand request,
        CancellationToken cancellationToken)
    {
        var affiliateSecret = await affiliateSecretRepository.GetByIdAsync(request.Id, cancellationToken);
        if (affiliateSecret == null)
        {
            return Result.Failure<Unit>("Affiliate secret not found", "NotFound");
        }

        await affiliateSecretRepository.DeleteAsync(affiliateSecret, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Unit.Value);
    }
}