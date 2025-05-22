using CRM.Identity.Application.Common.Specifications.Affiliates;
using CRM.Identity.Domain.Entities.Affiliate;

namespace CRM.Identity.Application.Features.Affiliates.Commands.UpdateAffiliate;

public sealed record UpdateAffiliateCommand(
    Guid Id,
    string Name,
    string Email,
    string? Phone,
    string? Website) : IRequest<Unit>;

public sealed class UpdateAffiliateCommandValidator : AbstractValidator<UpdateAffiliateCommand>
{
    public UpdateAffiliateCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Affiliate ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Name is required and cannot exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(200)
            .WithMessage("A valid email address is required.");

        RuleFor(x => x.Phone)
            .MaximumLength(20)
            .Matches(@"^[+]?[\d\s()-]+$")
            .When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Phone number format is invalid.");

        RuleFor(x => x.Website)
            .MaximumLength(200)
            .Matches(@"^https?://.+")
            .When(x => !string.IsNullOrEmpty(x.Website))
            .WithMessage("Website must be a valid URL.");
    }
}

public sealed class UpdateAffiliateCommandHandler(
    IRepository<Affiliate> affiliateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateAffiliateCommand, Unit>
{
    public async ValueTask<Result<Unit>> Handle(UpdateAffiliateCommand request, CancellationToken cancellationToken)
    {
        var affiliate = await affiliateRepository.GetByIdAsync(request.Id, cancellationToken);
        if (affiliate == null)
        {
            return Result.Failure<Unit>("Affiliate not found", "NotFound");
        }

        var emailSpecification =
            new AffiliateByEmailExcludingIdSpecification(request.Email.Trim().ToLower(), request.Id);
        var existingAffiliate = await affiliateRepository.FirstOrDefaultAsync(emailSpecification, cancellationToken);

        if (existingAffiliate != null)
        {
            return Result.Failure<Unit>("Affiliate with this email already exists", "Conflict");
        }

        affiliate.UpdateDetails(
            request.Name.Trim(),
            request.Email.Trim().ToLower(),
            request.Phone?.Trim(),
            request.Website?.Trim());

        await affiliateRepository.UpdateAsync(affiliate, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Unit.Value);
    }
}