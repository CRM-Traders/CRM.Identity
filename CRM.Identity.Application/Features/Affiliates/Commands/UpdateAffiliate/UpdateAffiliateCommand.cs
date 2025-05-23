using CRM.Identity.Domain.Entities.Affiliate;

namespace CRM.Identity.Application.Features.Affiliates.Commands.UpdateAffiliate;

public sealed record UpdateAffiliateCommand(
    Guid Id,
    string? Phone,
    string? Website) : IRequest<Unit>;

public sealed class UpdateAffiliateCommandValidator : AbstractValidator<UpdateAffiliateCommand>
{
    public UpdateAffiliateCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Affiliate ID is required.");


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

        affiliate.UpdateDetails(
            request.Phone?.Trim(),
            request.Website?.Trim());

        await affiliateRepository.UpdateAsync(affiliate, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Unit.Value);
    }
}