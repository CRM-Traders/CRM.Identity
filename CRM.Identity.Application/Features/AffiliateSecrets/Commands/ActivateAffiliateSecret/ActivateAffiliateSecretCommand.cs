using CRM.Identity.Domain.Entities.Affiliate;

namespace CRM.Identity.Application.Features.AffiliateSecrets.Commands.ActivateAffiliateSecret;

public sealed record ActivateAffiliateSecretCommand(Guid Id) : IRequest<Unit>;

public sealed class ActivateAffiliateSecretCommandValidator : AbstractValidator<ActivateAffiliateSecretCommand>
{
    public ActivateAffiliateSecretCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Affiliate Secret ID is required.");
    }
}

public sealed class ActivateAffiliateSecretCommandHandler(
    IRepository<AffiliateSecret> affiliateSecretRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ActivateAffiliateSecretCommand, Unit>
{
    public async ValueTask<Result<Unit>> Handle(ActivateAffiliateSecretCommand request,
        CancellationToken cancellationToken)
    {
        var affiliateSecret = await affiliateSecretRepository.GetByIdAsync(request.Id, cancellationToken);
        if (affiliateSecret == null)
        {
            return Result.Failure<Unit>("Affiliate secret not found", "NotFound");
        }

        affiliateSecret.Activate();

        await affiliateSecretRepository.UpdateAsync(affiliateSecret, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Unit.Value);
    }
}