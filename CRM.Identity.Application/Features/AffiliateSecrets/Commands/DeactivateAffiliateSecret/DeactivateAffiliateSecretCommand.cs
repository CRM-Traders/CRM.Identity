using CRM.Identity.Domain.Entities.Affiliate;

namespace CRM.Identity.Application.Features.AffiliateSecrets.Commands.DeactivateAffiliateSecret;

public sealed record DeactivateAffiliateSecretCommand(Guid Id) : IRequest<Unit>;

public sealed class DeactivateAffiliateSecretCommandValidator : AbstractValidator<DeactivateAffiliateSecretCommand>
{
    public DeactivateAffiliateSecretCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Affiliate Secret ID is required.");
    }
}

public sealed class DeactivateAffiliateSecretCommandHandler(
    IRepository<AffiliateSecret> affiliateSecretRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeactivateAffiliateSecretCommand, Unit>
{
    public async ValueTask<Result<Unit>> Handle(DeactivateAffiliateSecretCommand request, CancellationToken cancellationToken)
    {
        var affiliateSecret = await affiliateSecretRepository.GetByIdAsync(request.Id, cancellationToken);
        if (affiliateSecret == null)
        {
            return Result.Failure<Unit>("Affiliate secret not found", "NotFound");
        }

        affiliateSecret.Deactivate();

        await affiliateSecretRepository.UpdateAsync(affiliateSecret, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Unit.Value);
    }
}
