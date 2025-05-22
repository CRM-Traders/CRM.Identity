using CRM.Identity.Domain.Entities.Affiliate;

namespace CRM.Identity.Application.Features.AffiliateSecrets.Commands.UpdateAffiliateSecret;

public sealed record UpdateAffiliateSecretCommand(
    Guid Id,
    DateTimeOffset ExpirationDate,
    string? IpRestriction) : IRequest<Unit>;

public sealed class UpdateAffiliateSecretCommandValidator : AbstractValidator<UpdateAffiliateSecretCommand>
{
    public UpdateAffiliateSecretCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Affiliate Secret ID is required.");

        RuleFor(x => x.ExpirationDate)
            .GreaterThan(DateTimeOffset.UtcNow)
            .WithMessage("Expiration date must be in the future.");

        RuleFor(x => x.IpRestriction)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.IpRestriction))
            .WithMessage("IP restriction cannot exceed 500 characters.");
    }
}

public sealed class UpdateAffiliateSecretCommandHandler(
    IRepository<AffiliateSecret> affiliateSecretRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateAffiliateSecretCommand, Unit>
{
    public async ValueTask<Result<Unit>> Handle(UpdateAffiliateSecretCommand request,
        CancellationToken cancellationToken)
    {
        var affiliateSecret = await affiliateSecretRepository.GetByIdAsync(request.Id, cancellationToken);
        if (affiliateSecret == null)
        {
            return Result.Failure<Unit>("Affiliate secret not found", "NotFound");
        }

        affiliateSecret.UpdateExpiration(request.ExpirationDate);

        await affiliateSecretRepository.UpdateAsync(affiliateSecret, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Unit.Value);
    }
}