using CRM.Identity.Application.Common.Specifications.AffiliateSecrets;
using CRM.Identity.Domain.Entities.Affiliate;

namespace CRM.Identity.Application.Features.AffiliateSecrets.Commands.CreateAffiliateSecret;

public sealed record CreateAffiliateSecretCommand(
    Guid AffiliateId,
    string SecretKey,
    string ApiKey,
    DateTimeOffset ExpirationDate,
    string? IpRestriction) : IRequest<Guid>;

public sealed class CreateAffiliateSecretCommandValidator : AbstractValidator<CreateAffiliateSecretCommand>
{
    public CreateAffiliateSecretCommandValidator()
    {
        RuleFor(x => x.AffiliateId)
            .NotEmpty()
            .WithMessage("Affiliate ID is required.");

        RuleFor(x => x.SecretKey)
            .NotEmpty()
            .MinimumLength(32)
            .MaximumLength(128)
            .WithMessage("Secret key is required and must be between 32 and 128 characters.");

        RuleFor(x => x.ExpirationDate)
            .GreaterThan(DateTimeOffset.UtcNow)
            .WithMessage("Expiration date must be in the future.");

        RuleFor(x => x.IpRestriction)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.IpRestriction))
            .WithMessage("IP restriction cannot exceed 500 characters.");
    }
}

public sealed class CreateAffiliateSecretCommandHandler(
    IRepository<AffiliateSecret> affiliateSecretRepository,
    IRepository<Affiliate> affiliateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateAffiliateSecretCommand, Guid>
{
    public async ValueTask<Result<Guid>> Handle(CreateAffiliateSecretCommand request,
        CancellationToken cancellationToken)
    {
        var affiliate = await affiliateRepository.GetByIdAsync(request.AffiliateId, cancellationToken);
        if (affiliate == null)
        {
            return Result.Failure<Guid>("Affiliate not found", "NotFound");
        }

        var secretKeySpecification = new AffiliateSecretBySecretKeySpecification(request.SecretKey);
        var existingSecretByKey =
            await affiliateSecretRepository.FirstOrDefaultAsync(secretKeySpecification, cancellationToken);

        if (existingSecretByKey != null)
        {
            return Result.Failure<Guid>("Secret key already exists", "Conflict");
        }


        var affiliateSecret = new AffiliateSecret(
            request.AffiliateId,
            request.SecretKey,
            request.ExpirationDate,
            request.IpRestriction);

        await affiliateSecretRepository.AddAsync(affiliateSecret, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(affiliateSecret.Id);
    }
}